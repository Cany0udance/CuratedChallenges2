using CuratedChallenges.ChallengeUtil;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Runs;

namespace CuratedChallenges.Patches.SpecificChallenges.ShootingStar;

[HarmonyPatch(typeof(CardCmd))]
public static class CardCmdShootingStarPatch
{
    [HarmonyPatch("AutoPlay")]
    [HarmonyPrefix]
    static bool AutoPlayPrefix(
        PlayerChoiceContext choiceContext,
        CardModel card,
        ref Creature target,
        AutoPlayType type)
    {
        if (!IsShootingStarActive())
            return true; // Run original
        
        // Only modify Bombardment's auto-targeting
        if (!(card is Bombardment) || card.TargetType != TargetType.AnyEnemy)
            return true;
        
        // If no target specified, pick random target from enemies AND player
        if (target == null)
        {
            var combatState = card.CombatState ?? card.Owner?.Creature?.CombatState;
            if (combatState == null)
                return true;

            List<Creature> possibleTargets = new List<Creature>();
            possibleTargets.AddRange(combatState.HittableEnemies);

            if (card.Owner.Creature.IsAlive)
                possibleTargets.Add(card.Owner.Creature);

            if (possibleTargets.Count > 0)
            {
                target = card.Owner.RunState.Rng.CombatTargets.NextItem<Creature>((IEnumerable<Creature>)possibleTargets);
            }
        }
        
        return true;
    }
    
    private static bool IsShootingStarActive()
    {
        if (!RunManager.Instance.IsInProgress)
            return false;
        
        var runState = RunManager.Instance.DebugOnlyGetState();
        if (runState == null)
            return false;
        
        string challengeId = ChallengeRunTracker.GetChallengeIdFromRun(runState);
        return challengeId == "SHOOTING_STAR" || challengeId?.StartsWith("SHOOTING_STAR") == true;
    }
}