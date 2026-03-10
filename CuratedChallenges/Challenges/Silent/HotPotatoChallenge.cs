using CuratedChallenges.ChallengeUtil;
using CuratedChallenges.Modifiers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Models.Potions;
using MegaCrit.Sts2.Core.Models.Relics;

namespace CuratedChallenges.Challenges.Silent;

public class HotPotatoChallenge : ChallengeDefinition
{
    public override string Id => "HOT_POTATO";

    public override ModelId? CharacterId => ModelDb.GetId<MegaCrit.Sts2.Core.Models.Characters.Silent>();
    public override bool IsShared => false;

    public override IEnumerable<CardModel> GetStartingDeck(CharacterModel character)
    {
        var deck = new List<CardModel>();
    
        for (int i = 0; i < 4; i++)
            deck.Add(ModelDb.Card<StrikeSilent>());
    
        deck.Add(ModelDb.Card<DaggerThrow>());
    
        for (int i = 0; i < 4; i++)
            deck.Add(ModelDb.Card<DefendSilent>());
    
        deck.Add(ModelDb.Card<Prepared>());
        deck.Add(ModelDb.Card<Survivor>());
        deck.Add(ModelDb.Card<Neutralize>());
        deck.Add(ModelDb.Card<AscendersBane>());
    
        return deck;
    }

    public override IReadOnlyList<RelicModel> GetStartingRelics(CharacterModel character)
    {
        return new List<RelicModel>
        {
            ModelDb.Relic<RingOfTheSnake>(),
            ModelDb.Relic<DarkstonePeriapt>(),
            ModelDb.Relic<IronClub>()
        };
    }

    public override IEnumerable<ModelId> GetStartingPotions()
    {
        return new List<ModelId>
        {
            ModelDb.GetId<GamblersBrew>(),
            ModelDb.GetId<GamblersBrew>(),
            ModelDb.GetId<GamblersBrew>()
        };
    }
    
    public override ModifierModel CreateModifier()
    {
        var modifier = ModelDb.Modifier<HotPotatoModifier>().ToMutable() as HotPotatoModifier;
        modifier.SetChallenge(this);
        return modifier;
    }
}