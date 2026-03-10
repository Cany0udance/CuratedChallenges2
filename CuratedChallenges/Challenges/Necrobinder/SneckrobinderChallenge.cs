using CuratedChallenges.ChallengeUtil;
using CuratedChallenges.Modifiers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Models.Relics;

namespace CuratedChallenges.Challenges;

public class SneckrobinderChallenge : ChallengeDefinition
{
    public override string Id => "SNECKROBINDER";
    public override ModelId? CharacterId => ModelDb.GetId<Necrobinder>();
    public override bool IsShared => false;

    
    public override IEnumerable<CardModel> GetStartingDeck(CharacterModel character)
    {
        var deck = new List<CardModel>();
    
        for (int i = 0; i < 3; i++)
            deck.Add(ModelDb.Card<StrikeNecrobinder>());
    
        for (int i = 0; i < 3; i++)
            deck.Add(ModelDb.Card<DefendNecrobinder>());
    
        for (int i = 0; i < 2; i++)
            deck.Add(ModelDb.Card<BorrowedTime>());
    
        deck.Add(ModelDb.Card<Bodyguard>());
        
        deck.Add(ModelDb.Card<Unleash>());
    
        return deck;
    }

    public override IReadOnlyList<RelicModel> GetStartingRelics(CharacterModel character)
    {
        return new List<RelicModel>
        {
            ModelDb.Relic<BoundPhylactery>(),
            ModelDb.Relic<IntimidatingHelmet>()
        };
    }
    
    public override ModifierModel CreateModifier()
    {
        var modifier = ModelDb.Modifier<SneckrobinderModifier>().ToMutable() as SneckrobinderModifier;
        modifier.SetChallenge(this);
        return modifier;
    }
}