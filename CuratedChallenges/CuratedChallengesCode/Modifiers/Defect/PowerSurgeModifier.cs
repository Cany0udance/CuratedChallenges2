using CuratedChallenges.ChallengeUtil;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Afflictions;
using MegaCrit.Sts2.Core.ValueProps;

namespace CuratedChallenges.Modifiers;

public class PowerSurgeModifier : ChallengeModifier
{
public PowerSurgeModifier() : base() { }
    
    public PowerSurgeModifier(ChallengeDefinition challenge) : base(challenge) { }
    
    public override async Task AfterCardChangedPiles(CardModel card, PileType oldPile, AbstractModel? source)
    {
        await ApplyGalvanizedIfPower(card);
    }
    
    public override async Task AfterCardEnteredCombat(CardModel card)
    {
        await ApplyGalvanizedIfPower(card);
    }
    
    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        if (cardPlay.Card.Affliction is Galvanized galvanized)
        {
            VfxCmd.PlayOnCreature(cardPlay.Card.Owner.Creature, "vfx/vfx_attack_lightning");
            await CreatureCmd.Damage(context, cardPlay.Card.Owner.Creature, galvanized.Amount, 
                ValueProp.Unpowered | ValueProp.Move, null, null);
        }
    }
    
    private async Task ApplyGalvanizedIfPower(CardModel card)
    {
        if (card.Type == CardType.Power && 
            card.Affliction == null && 
            ModelDb.Affliction<Galvanized>().CanAfflict(card))
        {
            await CardCmd.Afflict<Galvanized>(card, 6m);
        }
    }
}