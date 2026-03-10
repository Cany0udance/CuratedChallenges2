using CuratedChallenges.ChallengeUtil;
using CuratedChallenges.Modifiers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Potions;
using MegaCrit.Sts2.Core.Models.Relics;

namespace CuratedChallenges.Challenges.Regent;

public class ShootingStarChallenge : ChallengeDefinition
{
    public override string Id => "SHOOTING_STAR";

    public override ModelId? CharacterId => ModelDb.GetId<MegaCrit.Sts2.Core.Models.Characters.Regent>();
    public override bool IsShared => false;
    
    
    public override IEnumerable<CardModel> GetStartingDeck(CharacterModel character)
    {
        var deck = new List<CardModel>();
        
        for (int i = 0; i < 4; i++)
            deck.Add(ModelDb.Card<StrikeRegent>());
        
        for (int i = 0; i < 3; i++)
            deck.Add(ModelDb.Card<DefendRegent>());
        
        deck.Add(ModelDb.Card<Glitterstream>());
        
        deck.Add(ModelDb.Card<FallingStar>());
        
        deck.Add(ModelDb.Card<Venerate>());
        
        return deck;
    }
    
    public override IReadOnlyList<RelicModel> GetStartingRelics(CharacterModel character)
    {
        return new List<RelicModel>
        {
            ModelDb.Relic<DivineRight>(),
            ModelDb.Relic<Anchor>(),
            ModelDb.Relic<HornCleat>(),
            ModelDb.Relic<CaptainsWheel>(),
            ModelDb.Relic<SturdyClamp>()
        };
    }
    
    public override ModifierModel CreateModifier()
    {
        var modifier = ModelDb.Modifier<ShootingStarModifier>().ToMutable() as ShootingStarModifier;
        modifier.SetChallenge(this);
        return modifier;
    }
}