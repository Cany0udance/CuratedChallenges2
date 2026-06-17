using CuratedChallenges.ChallengeUtil;
using CuratedChallenges.Modifiers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Models.Relics;

namespace CuratedChallenges.Challenges;

public class HardCarryChallenge : ChallengeDefinition
{
    public override string Id => "HARD_CARRY";
    public override ModelId? CharacterId => ModelDb.GetId<Necrobinder>();
    public override bool IsShared => false;
    
    public override IEnumerable<CardModel> GetStartingDeck(CharacterModel character)
    {
        var deck = new List<CardModel>();
        
        deck.Add(ModelDb.Card<StrikeNecrobinder>());
      
        deck.Add(ModelDb.Card<Fetch>());
      
        deck.Add(ModelDb.Card<Poke>());
        
        deck.Add(ModelDb.Card<SicEm>());
  
        for (int i = 0; i < 3; i++)
            deck.Add(ModelDb.Card<DefendNecrobinder>());
    
        var pullAggro = ModelDb.Card<PullAggro>().ToMutable();
        CardCmd.Enchant<RoyallyApproved>(pullAggro, 1M);
        CardCmd.Upgrade(pullAggro);
        deck.Add(pullAggro);
  
        deck.Add(ModelDb.Card<Bodyguard>());
        
        deck.Add(ModelDb.Card<Unleash>());
  
        return deck;
    }

    public override IReadOnlyList<RelicModel> GetStartingRelics(CharacterModel character)
    {
        return new List<RelicModel>
        {
            ModelDb.Relic<BoundPhylactery>(),
            ModelDb.Relic<BoneFlute>(),
            ModelDb.Relic<TungstenRod>(),
            ModelDb.Relic<TungstenRod>(),
            ModelDb.Relic<TungstenRod>()
        };
    }
    
    public override ModifierModel CreateModifier()
    {
        var modifier = ModelDb.Modifier<HardCarryModifier>().ToMutable() as HardCarryModifier;
        modifier.SetChallenge(this);
        return modifier;
    }
}