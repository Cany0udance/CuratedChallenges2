using CuratedChallenges.ChallengeUtil;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Runs;

namespace CuratedChallenges.Patches.SpecificChallenges.BurnBright;

[HarmonyPatch(typeof(ToyBox), nameof(ToyBox.AfterObtained))]
public static class ToyBoxAfterObtainedPatch
{
    [HarmonyPrefix]
    static bool Prefix(ref Task __result)
    {
        if (!IsBurnBrightActive())
            return true;

        __result = Task.CompletedTask;
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