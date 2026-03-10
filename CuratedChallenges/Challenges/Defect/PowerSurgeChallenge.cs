using CuratedChallenges.ChallengeUtil;
using CuratedChallenges.Modifiers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Relics;

namespace CuratedChallenges.Challenges.Defect;

public class PowerSurgeChallenge : ChallengeDefinition
{
    public override string Id => "POWER_SURGE";
    public override ModelId? CharacterId => ModelDb.GetId<MegaCrit.Sts2.Core.Models.Characters.Defect>();
    public override bool IsShared => false;
    
    public override IEnumerable<CardModel> GetStartingDeck(CharacterModel character)
    {
        var deck = new List<CardModel>();
        
        for (int i = 0; i < 2; i++)
            deck.Add(ModelDb.Card<StrikeDefect>());
        
        for (int i = 0; i < 2; i++)
             deck.Add(ModelDb.Card<Synthesis>());
        
        for (int i = 0; i < 3; i++)
            deck.Add(ModelDb.Card<DefendDefect>());
        
            deck.Add(ModelDb.Card<BoostAway>());
            
        deck.Add(ModelDb.Card<CreativeAi>());
        deck.Add(ModelDb.Card<Iteration>());
        
        return deck;
    }
    
    public override IReadOnlyList<RelicModel> GetStartingRelics(CharacterModel character)
    {
        return new List<RelicModel>
        {
            ModelDb.Relic<Permafrost>()
        };
    }
    
    public override ModifierModel CreateModifier()
    {
        var modifier = ModelDb.Modifier<PowerSurgeModifier>().ToMutable() as PowerSurgeModifier;
        modifier.SetChallenge(this);
        return modifier;
    }
}