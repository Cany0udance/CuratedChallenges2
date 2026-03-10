using MegaCrit.Sts2.Core.Runs;

namespace CuratedChallenges.ChallengeUtil;

public static class ChallengeRunTracker
{
    public static string GetChallengeIdFromRun(IRunState runState)
    {
        foreach (var modifier in runState.Modifiers)
        {
            if (modifier is ChallengeModifier challengeMod)
            {
                return challengeMod.ChallengeId;
            }
        
            // Check the store for DeprecatedModifiers
            var storedId = ChallengeIdStore.Get(modifier);
            if (!string.IsNullOrEmpty(storedId))
            {
                return storedId;
            }
        }
        return null;
    }
    
    public static bool IsChallengeRun(IRunState runState)
    {
        return GetChallengeIdFromRun(runState) != null;
    }
}