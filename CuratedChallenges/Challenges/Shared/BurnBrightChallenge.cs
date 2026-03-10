using CuratedChallenges.ChallengeUtil;
using CuratedChallenges.Modifiers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Models.Relics;

namespace CuratedChallenges.Challenges;

public class BurnBrightChallenge : ChallengeDefinition
{
    public override string Id => "BURN_BRIGHT";

    public override ModelId? CharacterId => null;  // Shared challenge
    public override bool IsShared => true;
    
    public override IReadOnlyList<RelicModel> GetStartingRelics(CharacterModel character)
    {
        var relics = new List<RelicModel>();
        
        relics.Add(ModelDb.Relic<ToyBox>());
        
        foreach (var relic in character.StartingRelics)
        {
            var waxRelic = ModelDb.GetById<RelicModel>(relic.Id).ToMutable();
            waxRelic.IsWax = true;
            relics.Add(waxRelic);
        }
        
        var wongoTicket = ModelDb.Relic<WongosMysteryTicket>().ToMutable();
        wongoTicket.IsWax = true;
        relics.Add(wongoTicket);

        return relics;
    }
    
    public override IEnumerable<string> GetBlacklistedEvents()
    {
        yield return ModelDb.Event<WelcomeToWongos>().Id.Entry;
    }
    
    public override ModifierModel CreateModifier()
    {
        var modifier = ModelDb.Modifier<BurnBrightModifier>().ToMutable() as BurnBrightModifier;
        modifier.SetChallenge(this);
        return modifier;
    }
}