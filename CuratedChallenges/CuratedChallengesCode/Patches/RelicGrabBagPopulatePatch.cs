using CuratedChallenges.ChallengeUtil;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;

namespace CuratedChallenges.Patches;

[HarmonyPatch(typeof(RelicGrabBag), nameof(RelicGrabBag.Populate), new[] { typeof(Player), typeof(Rng) })]
public static class RelicGrabBagPopulatePatch
{
    static void Postfix(RelicGrabBag __instance, Player player)
    {
        if (!RunManager.Instance.IsInProgress) return;
        
        var runState = RunManager.Instance.DebugOnlyGetState();
        if (runState == null || !ChallengeRunTracker.IsChallengeRun(runState)) return;
        
        var challengeMod = runState.Modifiers.OfType<ChallengeModifier>().FirstOrDefault();
        if (challengeMod == null) return;
        
        // Remove relics the player already owns
        foreach (var relic in player.Relics)
        {
            __instance.Remove(relic);
        }
        
        // Remove blacklisted relics
        foreach (var blacklistedId in challengeMod.Challenge.GetBlacklistedRelics())
        {
            var relic = ModelDb.GetByIdOrNull<RelicModel>(blacklistedId);
            if (relic != null)
            {
                __instance.Remove(relic);
            }
        }
    }
}