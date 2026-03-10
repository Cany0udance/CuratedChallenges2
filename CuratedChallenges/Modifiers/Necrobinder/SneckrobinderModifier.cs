using CuratedChallenges.ChallengeUtil;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Runs;

namespace CuratedChallenges.Modifiers;

public class SneckrobinderModifier : ChallengeModifier
{
public SneckrobinderModifier() : base() { }
    
    public SneckrobinderModifier(ChallengeDefinition challenge) : base(challenge) { }
    
    public override bool TryModifyCardRewardOptionsLate(
        Player player,
        List<CardCreationResult> cardRewards,
        CardCreationOptions options)
    {
        Slither slither = ModelDb.Enchantment<Slither>();
        bool modified = false;
    
        foreach (CardCreationResult cardReward in cardRewards)
        {
            CardModel card = cardReward.Card;
        
            // Check if card costs 0 or 1
            if ((card.EnergyCost.Canonical == 0 || card.EnergyCost.Canonical == 1) && slither.CanEnchant(card))
            {
                CardModel clonedCard = player.RunState.CloneCard(card);
                CardCmd.Enchant<Slither>(clonedCard, 1M);
                cardReward.ModifyCard(clonedCard);
                modified = true;
            }
        }
    
        return modified;
    }
}