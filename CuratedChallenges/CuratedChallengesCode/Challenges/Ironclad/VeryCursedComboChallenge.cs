using CuratedChallenges.ChallengeUtil;
using CuratedChallenges.Modifiers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Models.Relics;

namespace CuratedChallenges.Challenges;

public class VeryCursedComboChallenge : ChallengeDefinition
{
    public override string Id => "VERY_CURSED_COMBO";
    public override ModelId? CharacterId => ModelDb.GetId<Ironclad>();
    public override bool IsShared => false;
    public override bool AllowNeowBonuses => true;
    public override string HiddenUntilChallengeId => "CURSED_COMBO"; // The prerequisite
    
    
    public override IEnumerable<CardModel> GetStartingDeck(CharacterModel character)
    {
        var deck = new List<CardModel>();
        
        for (int i = 0; i < 5; i++)
            deck.Add(ModelDb.Card<StrikeIronclad>());
        
        deck.Add(ModelDb.Card<Cinder>());
        
        for (int i = 0; i < 4; i++)
            deck.Add(ModelDb.Card<DefendIronclad>());
        
        deck.Add(ModelDb.Card<Bash>());
        
        return deck;
    }
    
    public override IReadOnlyList<RelicModel> GetStartingRelics(CharacterModel character)
    {
        return new List<RelicModel>
        {
            ModelDb.Relic<FakeSneckoEye>()
        };
    }
    
    public override IEnumerable<ModelId> GetBlacklistedRelics()
    {
        yield return ModelDb.GetId<RunicPyramid>();
    }
    
    public override ModifierModel CreateModifier()
    {
        var modifier = ModelDb.Modifier<VeryCursedComboModifier>().ToMutable() as VeryCursedComboModifier;
        modifier.SetChallenge(this);
        return modifier;
    }
}