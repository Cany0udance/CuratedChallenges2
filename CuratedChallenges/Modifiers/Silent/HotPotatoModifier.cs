using CuratedChallenges.ChallengeUtil;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;

namespace CuratedChallenges.Modifiers;

public class HotPotatoModifier : ChallengeModifier
{
public HotPotatoModifier() : base() { }
    
    public HotPotatoModifier(ChallengeDefinition challenge) : base(challenge) { }
    
    public override async Task AfterCardExhausted(
        PlayerChoiceContext choiceContext,
        CardModel card,
        bool causedByEthereal)
    {
        await base.AfterCardExhausted(choiceContext, card, causedByEthereal);
        
        // Check if the exhausted card is a curse
        if (card.Rarity != CardRarity.Curse)
            return;
        
        // Get the player who owns this card
        Player player = card.Owner;
        if (player?.Creature == null)
            return;
        
        // Lose 20 max HP
        await CreatureCmd.LoseMaxHp(choiceContext, player.Creature, 20, true);
    }
}