using CuratedChallenges.CuratedChallengesCode;
using MegaCrit.Sts2.Core.Models;

namespace CuratedChallenges.ChallengeUtil.Progression;

public static class ChallengeProgressHelper
{
    public static bool IsChallengeCompleted(string challengeId, bool isShared, ModelId characterId)
    {
        string key = isShared 
            ? $"{challengeId}_{characterId.Entry}" 
            : challengeId;
        return CuratedChallengesConfig.GetCompletedChallenges().ContainsKey(key);
    }

    public static int GetHighestAscension(string challengeId, bool isShared, ModelId characterId)
    {
        string key = isShared 
            ? $"{challengeId}_{characterId.Entry}" 
            : challengeId;
        var completed = CuratedChallengesConfig.GetCompletedChallenges();
        return completed.TryGetValue(key, out int ascension) ? ascension : 0;
    }
}