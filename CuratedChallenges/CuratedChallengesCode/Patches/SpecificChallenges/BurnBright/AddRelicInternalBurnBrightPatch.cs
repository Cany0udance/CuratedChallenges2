using CuratedChallenges.ChallengeUtil;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Runs;

namespace CuratedChallenges.Patches.SpecificChallenges.BurnBright;

[HarmonyPatch(typeof(Player), nameof(Player.AddRelicInternal))]
public static class AddRelicInternalBurnBrightPatch
{
    static void Prefix(RelicModel relic)
    {
        if (!IsBurnBrightActive())
            return;
        
        // Don't mark ToyBox itself as wax - it should never melt
        if (relic is ToyBox)
            return;
        
        relic.IsWax = true;
    }
    
    private static bool IsBurnBrightActive()
    {
        if (!RunManager.Instance.IsInProgress)
            return false;
        
        var runState = RunManager.Instance.DebugOnlyGetState();
        if (runState == null)
            return false;
        
        string challengeId = ChallengeRunTracker.GetChallengeIdFromRun(runState);
        return challengeId == "BURN_BRIGHT" || challengeId?.StartsWith("BURN_BRIGHT") == true;
    }
}