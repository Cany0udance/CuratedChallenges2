using System.Reflection;
using System.Reflection.Emit;
using CuratedChallenges.ChallengeUtil;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Runs;

namespace CuratedChallenges.Patches.SpecificChallenges.VakuuTakeTheWheel;

[HarmonyPatch(typeof(WhisperingEarring), nameof(WhisperingEarring.BeforePlayPhaseStart))]
public static class WhisperingEarringVakuuTakeTheWheelPatch
{
    static bool Prefix(WhisperingEarring __instance, PlayerChoiceContext choiceContext, Player player, ref Task __result)
    {
        if (!IsVakuuTakeTheWheelActive())
            return true; // Run original method
        
        if (player != __instance.Owner)
            return true;
        
        __result = ModifiedBeforePlayPhaseStart(__instance, choiceContext, player);
        return false;
    }
    
    private static async Task ModifiedBeforePlayPhaseStart(WhisperingEarring relic, PlayerChoiceContext choiceContext, Player player)
    {
        var combatState = player.Creature.CombatState;
        
        using (CardSelectCmd.PushSelector(new VakuuCardSelector()))
        {
            int cardsPlayed = 0;
            while (cardsPlayed < 13 && !CombatManager.Instance.IsOverOrEnding)
            {
                var card = PileType.Hand.GetPile(player).Cards.FirstOrDefault(c => c.CanPlay());
                if (card == null)
                    break;
                
                var target = GetTarget(relic, card, combatState);
                await card.SpendResources();
                await CardCmd.AutoPlay(choiceContext, card, target, skipXCapture: true);
                cardsPlayed++;
            }
        }
    }
    
    private static Creature GetTarget(WhisperingEarring relic, CardModel card, CombatState combatState)
    {
        var combatTargets = relic.Owner.RunState.Rng.CombatTargets;
        
        return card.TargetType switch
        {
            TargetType.AnyEnemy => combatState.HittableEnemies.FirstOrDefault(),
            TargetType.AnyPlayer => relic.Owner.Creature,
            TargetType.AnyAlly => combatTargets.NextItem(
                combatState.Allies.Where(c => c.IsAlive && c != relic.Owner.Creature)
            ),
            _ => null
        };
    }
    
    public static bool IsVakuuTakeTheWheelActive()
    {
        if (!RunManager.Instance.IsInProgress)
            return false;

        var runState = RunManager.Instance.DebugOnlyGetState();
        if (runState == null)
            return false;

        string challengeId = ChallengeRunTracker.GetChallengeIdFromRun(runState);
        return challengeId == "VAKUU_TAKE_THE_WHEEL" || challengeId == "VAKUU_TAKE_THE_PRISMATIC_WHEEL";
    }
}