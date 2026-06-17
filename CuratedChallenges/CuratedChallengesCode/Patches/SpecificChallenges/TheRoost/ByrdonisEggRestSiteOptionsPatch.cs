using CuratedChallenges.ChallengeUtil;
using CuratedChallenges.Modifiers;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Runs;

namespace CuratedChallenges.Patches.SpecificChallenges.TheRoost;

[HarmonyPatch(typeof(ByrdonisEgg), nameof(ByrdonisEgg.TryModifyRestSiteOptions))]
public static class ByrdonisEggRestSiteOptionsPatch
{
    static bool Prefix(ByrdonisEgg __instance, Player player, ICollection<RestSiteOption> options, ref bool __result)
    {
        // Only apply this patch if The Roost challenge is active
        if (!IsTheRoostActive())
            return true; // Use original behavior
        
        if (player != __instance.Owner)
        {
            __result = false;
            return false;
        }
        
        // Only add the Hatch option if it's not already present
        if (!options.Any(opt => opt.OptionId == "HATCH"))
        {
            options.Add(new HatchRestSiteOption(player));
        }
        
        __result = true;
        return false; // Skip original method
    }
    
    private static bool IsTheRoostActive()
    {
        if (!RunManager.Instance.IsInProgress)
            return false;
    
        var runState = RunManager.Instance.DebugOnlyGetState();
        if (runState == null)
            return false;
    
        string challengeId = ChallengeRunTracker.GetChallengeIdFromRun(runState);
        
        return challengeId == "THE_ROOST" || challengeId?.StartsWith("THE_ROOST") == true;
    }
}