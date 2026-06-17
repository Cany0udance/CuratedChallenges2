using System.Reflection;
using CuratedChallenges.ChallengeUtil;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Runs;

namespace CuratedChallenges.Patches;

[HarmonyPatch(typeof(AncientEventModel), "GenerateInitialOptionsWrapper")]
public static class AncientEventDuplicateRelicPatch
{
    private const string LOG_TAG = "[AncientSwap]";
    private const int MaxRerollAttempts = 50;
    
    private static readonly MethodInfo GenerateInitialOptionsMethod =
        typeof(AncientEventModel).GetMethod("GenerateInitialOptions",
            BindingFlags.Instance | BindingFlags.NonPublic);
    
    private static readonly PropertyInfo GeneratedOptionsProperty =
        typeof(AncientEventModel).GetProperty("GeneratedOptions",
            BindingFlags.Instance | BindingFlags.NonPublic);
    
    static void Postfix(AncientEventModel __instance,
        ref IReadOnlyList<EventOption> __result)
    {
        if (!RunManager.Instance.IsInProgress) return;
        
        var runState = RunManager.Instance.DebugOnlyGetState();
        if (runState == null || !ChallengeRunTracker.IsChallengeRun(runState)) return;
        
        var challengeMod = runState.Modifiers.OfType<ChallengeModifier>().FirstOrDefault();
        
        var player = __instance.Owner;
        var ownedRelicIds = new HashSet<ModelId>(
            player.Relics.Select(r => r.Id));
        
        var blacklistedIds = challengeMod != null
            ? new HashSet<ModelId>(challengeMod.Challenge.GetBlacklistedRelics())
            : new HashSet<ModelId>();
        
        if (!__result.Any(o =>
            o.Relic != null &&
            (ownedRelicIds.Contains(o.Relic.Id) || blacklistedIds.Contains(o.Relic.Id))))
            return;
        
        var excludedIds = new HashSet<ModelId>(ownedRelicIds);
        excludedIds.UnionWith(blacklistedIds);
        
        var rerolled = TryReroll(__instance, excludedIds);
        if (rerolled != null)
        {
            SyncGeneratedOptions(__instance, rerolled);
            __result = rerolled;
        }
    }
    
    private static IReadOnlyList<EventOption> TryReroll(
        AncientEventModel instance,
        HashSet<ModelId> excludedRelicIds)
    {
        for (int attempt = 0; attempt < MaxRerollAttempts; attempt++)
        {
            IReadOnlyList<EventOption> options;
            try
            {
                options = (IReadOnlyList<EventOption>)
                    GenerateInitialOptionsMethod.Invoke(instance, null);
            }
            catch (Exception e)
            {
                Log.Error($"{LOG_TAG} Failed to reroll options: {e}");
                return null;
            }
            
            if (options == null || options.Count == 0)
                continue;
            
            bool hasExcluded = options.Any(o =>
                o.Relic != null && excludedRelicIds.Contains(o.Relic.Id));
            
            if (!hasExcluded)
                return options;
        }
        
        return null;
    }
    
    private static void SyncGeneratedOptions(
        AncientEventModel instance,
        IReadOnlyList<EventOption> options)
    {
        try
        {
            GeneratedOptionsProperty.SetValue(
                instance, options.ToList());
        }
        catch (Exception e)
        {
            Log.Error(
                $"{LOG_TAG} Failed to sync GeneratedOptions: {e}");
        }
    }
}