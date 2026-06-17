using CuratedChallenges.ChallengeUtil;
using CuratedChallenges.Modifiers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Relics;

namespace CuratedChallenges.Challenges;

public class VakuuTakeThePrismaticWheelChallenge : ChallengeDefinition
{
    public override string Id => "VAKUU_TAKE_THE_PRISMATIC_WHEEL";
    public override ModelId? CharacterId => null;
    public override bool IsShared => true;
    public override string HiddenUntilChallengeId => "VAKUU_TAKE_THE_WHEEL"; // The prerequisite
    
    public override IReadOnlyList<RelicModel> GetStartingRelics(CharacterModel character)
    {
        var relics = new List<RelicModel>();
        
        foreach (var relic in character.StartingRelics)
        {
            relics.Add(ModelDb.GetById<RelicModel>(relic.Id));
        }
        
        relics.Add(ModelDb.Relic<WhisperingEarring>());
        relics.Add(ModelDb.Relic<Driftwood>());
        relics.Add(ModelDb.Relic<PrismaticGem>());
        
        return relics;
    }
    
    public override IEnumerable<CardModel> GetStartingDeck(CharacterModel character)
    {
        var deck = new List<CardModel>();
        
        foreach (var card in character.StartingDeck)
        {
            deck.Add(ModelDb.GetById<CardModel>(card.Id));
        }
        
        var production = ModelDb.Card<Production>().ToMutable();
        production.UpgradeInternal();
        production.FinalizeUpgradeInternal();
        deck.Add(production);
        
        return deck;
    }
    
    public override ModifierModel CreateModifier()
    {
        var modifier = ModelDb.Modifier<VakuuTakeThePrismaticWheelModifier>().ToMutable() as VakuuTakeThePrismaticWheelModifier;
        modifier.SetChallenge(this);
        return modifier;
    }
}