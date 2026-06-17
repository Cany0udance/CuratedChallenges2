using CuratedChallenges.ChallengeUtil;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Runs;

namespace CuratedChallenges.Patches.SpecificChallenges.VakuuTakeTheWheel;

[HarmonyPatch(typeof(NEndTurnButton), "OnTurnStarted")]
public static class VakuuAutoEndTurnPatch
{
    static async void Postfix(NEndTurnButton __instance, CombatState state)
    {
        if (!VakuuAutoEndState.IsEnabled) return;
        if (!WhisperingEarringVakuuTakeTheWheelPatch.IsVakuuTakeTheWheelActive()) return;
        if (state.CurrentSide != CombatSide.Player) return;
        
        var player = LocalContext.GetMe((ICombatState)state);
        
        // Wait for WhisperingEarring autoplay to finish + player reaction time
        await __instance.ToSignal(
            __instance.GetTree().CreateTimer(1.5),
            SceneTreeTimer.SignalName.Timeout);
        
        if (!VakuuAutoEndState.IsEnabled) return;
        if (CombatManager.Instance.IsOverOrEnding) return;
        if (CombatManager.Instance.IsPlayerReadyToEndTurn(player)) return;
        if (PileType.Hand.GetPile(player).Cards.Any(c => c.CanPlay())) return;
        
        RunManager.Instance.ActionQueueSynchronizer.RequestEnqueue(
            new EndPlayerTurnAction(player, player.PlayerCombatState.TurnNumber));
    }
}