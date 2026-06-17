using CuratedChallenges.ChallengeUtil;
using CuratedChallenges.Modifiers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Models.Potions;
using MegaCrit.Sts2.Core.Models.Relics;

namespace CuratedChallenges.Challenges.Regent;

public class FastTrackChallenge : ChallengeDefinition
{
    public override string Id => "FAST_TRACK";
    public override ModelId? CharacterId => ModelDb.GetId<MegaCrit.Sts2.Core.Models.Characters.Regent>();
    public override bool IsShared => false;
    
    public override IEnumerable<CardModel> GetStartingDeck(CharacterModel character)
    {
        var deck = new List<CardModel>();
    
        // Helper method to create and upgrade a card
        CardModel CreateUpgradedCard<T>() where T : CardModel
        {
            var card = ModelDb.Card<T>().ToMutable();
            card.UpgradeInternal();
            card.FinalizeUpgradeInternal();
            return card;
        }
    
        for (int i = 0; i < 4; i++)
            deck.Add(CreateUpgradedCard<StrikeRegent>());
    
        for (int i = 0; i < 4; i++)
            deck.Add(CreateUpgradedCard<DefendRegent>());
    
        deck.Add(CreateUpgradedCard<FallingStar>());
        deck.Add(CreateUpgradedCard<Venerate>());
    
        return deck;
    }
    
    public override IReadOnlyList<RelicModel> GetStartingRelics(CharacterModel character)
    {
        return new List<RelicModel>
        {
            ModelDb.Relic<DivineRight>()
        };
    }
    
    public override IEnumerable<ModelId> GetStartingPotions()
    {
        return new List<ModelId>
        {
            ModelDb.GetId<CureAll>(),
            ModelDb.GetId<CureAll>(),
            ModelDb.GetId<CureAll>()
        };
    }
    
    public override ModifierModel CreateModifier()
    {
        var modifier = ModelDb.Modifier<FastTrackModifier>().ToMutable() as FastTrackModifier;
        modifier.SetChallenge(this);
        return modifier;
    }
}