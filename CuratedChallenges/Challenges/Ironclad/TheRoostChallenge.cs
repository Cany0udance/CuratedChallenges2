using CuratedChallenges.ChallengeUtil;
using CuratedChallenges.Modifiers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Models.Relics;

namespace CuratedChallenges.Challenges;

public class TheRoostChallenge : ChallengeDefinition
{
    public override string Id => "THE_ROOST";
    public override ModelId? CharacterId => ModelDb.GetId<Ironclad>();
    public override bool IsShared => false;
    
    public override IEnumerable<CardModel> GetStartingDeck(CharacterModel character)
    {
        var deck = new List<CardModel>();
        
        for (int i = 0; i < 3; i++)
            deck.Add(ModelDb.Card<StrikeIronclad>());
        
        for (int i = 0; i < 2; i++)
            deck.Add(ModelDb.Card<PommelStrike>());
        
        for (int i = 0; i < 2; i++)
            deck.Add(ModelDb.Card<DefendIronclad>());
        
        for (int i = 0; i < 2; i++)
            deck.Add(ModelDb.Card<TrueGrit>());
        
        deck.Add(ModelDb.Card<Bash>());
        
        for (int i = 0; i < 5; i++)
            deck.Add(ModelDb.Card<ByrdonisEgg>());
        
        return deck;
    }
    
    public override IReadOnlyList<RelicModel> GetStartingRelics(CharacterModel character)
    {
        return new List<RelicModel>
        {
            ModelDb.Relic<BagOfPreparation>(),
            ModelDb.Relic<PaelsEye>()
        };
    }
    
    public override IEnumerable<string> GetBlacklistedEvents()
    {
        yield return ModelDb.Event<ByrdonisNest>().Id.Entry;
    }
    
    public override ModifierModel CreateModifier()
    {
        var modifier = ModelDb.Modifier<TheRoostModifier>().ToMutable() as TheRoostModifier;
        modifier.SetChallenge(this);
        return modifier;
    }
}