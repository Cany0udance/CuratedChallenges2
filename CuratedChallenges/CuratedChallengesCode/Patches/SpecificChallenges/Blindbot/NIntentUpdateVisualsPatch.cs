using System.Reflection;
using CuratedChallenges.ChallengeUtil;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Runs;

namespace CuratedChallenges.Patches.SpecificChallenges.Blindbot;

[HarmonyPatch(typeof(NIntent))]
public static class NIntentBlindBotPatch
{
    [HarmonyPatch("_Ready")]
    [HarmonyPostfix]
    static void ReadyPostfix(NIntent __instance)
    {
        if (!IsBlindBotActive())
            return;
        
        // Hide the entire intent UI element
        __instance.Visible = false;
    }
    
    private static bool IsBlindBotActive()
    {
        if (!RunManager.Instance.IsInProgress)
            return false;
        
        var runState = RunManager.Instance.DebugOnlyGetState();
        if (runState == null)
            return false;
        
        string challengeId = ChallengeRunTracker.GetChallengeIdFromRun(runState);
        return challengeId == "BLINDBOT" || challengeId?.StartsWith("BLINDBOT") == true;
    }
}