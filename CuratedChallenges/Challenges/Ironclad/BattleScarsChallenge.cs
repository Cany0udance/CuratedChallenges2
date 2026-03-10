using CuratedChallenges.ChallengeUtil;
using CuratedChallenges.Modifiers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Models.Relics;

namespace CuratedChallenges.Challenges;

public class BattleScarsChallenge : ChallengeDefinition
{
    public override string Id => "BATTLE_SCARS";
    public override ModelId? CharacterId => ModelDb.GetId<Ironclad>();
    public override bool IsShared => false;
    
    public override IEnumerable<CardModel> GetStartingDeck(CharacterModel character)
    {
        var deck = new List<CardModel>();
        
        for (int i = 0; i < 4; i++)
            deck.Add(ModelDb.Card<StrikeIronclad>());
        
        for (int i = 0; i < 3; i++)
            deck.Add(ModelDb.Card<DefendIronclad>());
        
        deck.Add(ModelDb.Card<FightMe>());
        deck.Add(ModelDb.Card<Bash>());
        deck.Add(ModelDb.Card<TrueGrit>());
        
        return deck;
    }
    
    public override IReadOnlyList<RelicModel> GetStartingRelics(CharacterModel character)
    {
        return new List<RelicModel>
        {
            ModelDb.Relic<MeatOnTheBone>()
        };
    }
    
    public override ModifierModel CreateModifier()
    {
        var modifier = ModelDb.Modifier<BattleScarsModifier>().ToMutable() as BattleScarsModifier;
        modifier.SetChallenge(this);
        return modifier;
    }
}