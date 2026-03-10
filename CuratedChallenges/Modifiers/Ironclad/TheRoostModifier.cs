using CuratedChallenges.ChallengeUtil;
using CuratedChallenges.WinConditions;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;

namespace CuratedChallenges.Modifiers;

public class TheRoostModifier : ChallengeModifier
{
public TheRoostModifier() : base() { }
    
    public TheRoostModifier(ChallengeDefinition challenge) : base(challenge) { }
    
    public override IEnumerable<IWinCondition> WinConditions
    {
        get
        {
            yield return new ObtainRelicWinCondition(new ModelId("RELIC", "BYRDPIP"), 5);
        }
    }
    
    protected override void OnChallengeRunCreated(RunState runState)
    {
        
    }
}