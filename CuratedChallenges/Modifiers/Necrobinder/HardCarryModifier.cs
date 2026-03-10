using CuratedChallenges.ChallengeUtil;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Runs;

namespace CuratedChallenges.Modifiers;

public class HardCarryModifier : ChallengeModifier
{
public HardCarryModifier() : base() { }
    
    public HardCarryModifier(ChallengeDefinition challenge) : base(challenge) { }
    
    protected override void OnChallengeRunCreated(RunState runState)
    {
        Player player = runState.Players[0];
        int currentMaxHp = player.Creature.MaxHp;
        int hpToLose = currentMaxHp - 1;
        
        if (hpToLose > 0)
        {
            TaskHelper.RunSafely(CreatureCmd.LoseMaxHp(
                new ThrowingPlayerChoiceContext(), 
                player.Creature, 
                hpToLose, 
                false
            ));
        }
    }
}