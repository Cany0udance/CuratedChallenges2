using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;

namespace CuratedChallenges.ChallengeUtil;

public static class ChallengeRegistry
{
    private static readonly Dictionary<string, ChallengeDefinition> _challenges = new Dictionary<string, ChallengeDefinition>();
    
    private static bool _initialized;

    public static event Action OnRegistryReady;

    public static bool IsInitialized => _initialized;

    public static void MarkInitialized()
    {
        _initialized = true;
        OnRegistryReady?.Invoke();
    }
    
    public static void RegisterChallenge(ChallengeDefinition challenge)
    {
        _challenges[challenge.Id] = challenge;
    }
    
    public static ChallengeDefinition GetChallenge(string id)
    {
        return _challenges.TryGetValue(id, out var challenge) ? challenge : null;
    }
    
    public static IEnumerable<ChallengeDefinition> GetChallengesForCharacter(ModelId characterId)
    {
        return _challenges.Values.Where(c => 
            (c.CharacterId == characterId || c.IsShared) && !c.IsHidden(characterId));
    }
}