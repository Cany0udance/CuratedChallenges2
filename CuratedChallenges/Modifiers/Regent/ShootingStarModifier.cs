using CuratedChallenges.ChallengeUtil;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;

namespace CuratedChallenges.Modifiers;

public class ShootingStarModifier : ChallengeModifier
{
public ShootingStarModifier() : base() { }
    
    public ShootingStarModifier(ChallengeDefinition challenge) : base(challenge) { }
    
    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature.CombatState.RoundNumber != 1)
            return;
        
        CardModel bombardment = player.Creature.CombatState.CreateCard(
            ModelDb.Card<Bombardment>(), 
            player
        );
        
        await CardCmd.AutoPlay(choiceContext, bombardment, (Creature) null);
    }
}