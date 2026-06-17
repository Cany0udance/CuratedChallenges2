using CuratedChallenges.ChallengeUtil;
using CuratedChallenges.Modifiers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Models.Relics;

namespace CuratedChallenges.Challenges;

public class SplitPersonalityChallenge : ChallengeDefinition
{
    public override string Id => "SPLIT_PERSONALITY";

    public override ModelId? CharacterId => null;  // Shared challenge
    public override bool IsShared => true;
    
    public override IEnumerable<ModelId> GetBlacklistedRelics()
    {
        yield return ModelDb.GetId<SeaGlass>();
    }
    
    public override ModifierModel CreateModifier()
    {
        var modifier = ModelDb.Modifier<SplitPersonalityModifier>().ToMutable() as SplitPersonalityModifier;
        modifier.SetChallenge(this);
        return modifier;
    }
}