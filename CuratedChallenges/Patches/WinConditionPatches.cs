using CuratedChallenges.ChallengeUtil;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Runs;

namespace CuratedChallenges.Patches;

[HarmonyPatch(typeof(Player), nameof(Player.AddRelicInternal))]
public static class AddRelicInternalPatch
{
    static void Postfix(Player __instance)
    {
        CheckChallengeWinConditions(__instance);
    }
    
    private static void CheckChallengeWinConditions(Player player)
    {
        if (!RunManager.Instance.IsInProgress) return;
        
        var runState = RunManager.Instance.DebugOnlyGetState();
        if (runState == null) return;
        
        var challengeMod = runState.Modifiers.OfType<ChallengeModifier>().FirstOrDefault();
        challengeMod?.CheckWinConditions(runState, player);
    }
}
