using CuratedChallenges.ChallengeUtil;
using CuratedChallenges.Modifiers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Models.Relics;

namespace CuratedChallenges.Challenges.Defect;

public class BlindbotChallenge : ChallengeDefinition
{
    public override string Id => "BLINDBOT";
    public override ModelId? CharacterId => ModelDb.GetId<MegaCrit.Sts2.Core.Models.Characters.Defect>();
    public override bool IsShared => false;
    
    public override IEnumerable<CardModel> GetStartingDeck(CharacterModel character)
    {
        var deck = new List<CardModel>();
        
        for (int i = 0; i < 3; i++)
            deck.Add(ModelDb.Card<StrikeDefect>());
        
        for (int i = 0; i < 3; i++)
            deck.Add(ModelDb.Card<DefendDefect>());
        
        for (int i = 0; i < 2; i++)
            deck.Add(ModelDb.Card<GoForTheEyes>());
        
        deck.Add(ModelDb.Card<Zap>());
        deck.Add(ModelDb.Card<Dualcast>());
        
        return deck;
    }
    
    public override IReadOnlyList<RelicModel> GetStartingRelics(CharacterModel character)
    {
        return new List<RelicModel>
        {
            ModelDb.Relic<CrackedCore>(),
            ModelDb.Relic<LavaLamp>()
        };
    }
    
    public override ModifierModel CreateModifier()
    {
        var modifier = ModelDb.Modifier<BlindbotModifier>().ToMutable() as BlindbotModifier;
        modifier.SetChallenge(this);
        return modifier;
    }
}