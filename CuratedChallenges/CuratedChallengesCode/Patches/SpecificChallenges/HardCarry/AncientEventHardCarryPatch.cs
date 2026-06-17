using CuratedChallenges.ChallengeUtil;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Runs;

namespace CuratedChallenges.Patches.SpecificChallenges.HardCarry;
[HarmonyPatch(typeof(AncientEventModel), "BeforeEventStarted", MethodType.Normal)]
public static class AncientEventHardCarryPatch
{
    static async Task Postfix(Task __result, AncientEventModel __instance)
    {
        await __result; // Wait for the original method to complete
        
        // Only apply to Neow
        if (!(__instance is Neow))
            return;
        
        if (!IsHardCarryActive())
            return;
        
        // Force HP to 1 after Neow's healing
        __instance.Owner.Creature.SetCurrentHpInternal(1M);
    }
    
    private static bool IsHardCarryActive()
    {
        if (!RunManager.Instance.IsInProgress)
            return false;
    
        var runState = RunManager.Instance.DebugOnlyGetState();
        if (runState == null)
            return false;
    
        string challengeId = ChallengeRunTracker.GetChallengeIdFromRun(runState);
        return challengeId == "HARD_CARRY";
    }
}