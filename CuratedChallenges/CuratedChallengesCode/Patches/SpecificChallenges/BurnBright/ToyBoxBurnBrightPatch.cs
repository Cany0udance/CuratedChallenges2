using CuratedChallenges.ChallengeUtil;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Runs;

namespace CuratedChallenges.Patches.SpecificChallenges.BurnBright;

[HarmonyPatch(typeof(ToyBox))]
public static class ToyBoxBurnBrightPatch
{
    [HarmonyPatch(nameof(ToyBox.IsUsedUp), MethodType.Getter)]
    [HarmonyPrefix]
    static bool IsUsedUpPrefix(ref bool __result)
    {
        if (!IsBurnBrightActive())
            return true;
        
        // Toy Box never expires in Burn Bright
        __result = false;
        return false;
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
