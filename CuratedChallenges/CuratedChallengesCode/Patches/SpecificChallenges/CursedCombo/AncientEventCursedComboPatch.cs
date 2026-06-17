using CuratedChallenges.ChallengeUtil;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Runs;

namespace CuratedChallenges.Patches.SpecificChallenges.CursedCombo;

[HarmonyPatch(typeof(AncientEventModel), "BeforeEventStarted", MethodType.Normal)]
public static class AncientEventCursedComboPatch
{
    static async Task Postfix(Task __result, AncientEventModel __instance)
    {
        await __result;
        
        if (!IsCursedComboActive())
            return;
        
        var runState = RunManager.Instance.DebugOnlyGetState();
        if (runState == null)
            return;
        
        if (runState.CurrentActIndex == 1)
        {
            RunicPyramid runicPyramid = await RelicCmd.Obtain<RunicPyramid>(__instance.Owner);
        }
    }
    
    private static bool IsCursedComboActive()
    {
        if (!RunManager.Instance.IsInProgress)
            return false;
    
        var runState = RunManager.Instance.DebugOnlyGetState();
        if (runState == null)
            return false;
    
        string challengeId = ChallengeRunTracker.GetChallengeIdFromRun(runState);
        return challengeId == "CURSED_COMBO" || challengeId == "VERY_CURSED_COMBO";
    }
}