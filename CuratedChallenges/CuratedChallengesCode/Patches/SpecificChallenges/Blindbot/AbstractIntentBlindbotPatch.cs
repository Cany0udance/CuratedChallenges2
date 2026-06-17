using CuratedChallenges.ChallengeUtil;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.Runs;

namespace CuratedChallenges.Patches.SpecificChallenges.Blindbot;


[HarmonyPatch(typeof(AbstractIntent))]
public static class AbstractIntentBlindBotPatch
{
    [HarmonyPatch(nameof(AbstractIntent.HasIntentTip), MethodType.Getter)]
    [HarmonyPrefix]
    static bool HasIntentTipPrefix(ref bool __result)
    {
        if (!IsBlindBotActive())
            return true;
        
        __result = false;
        return false;
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