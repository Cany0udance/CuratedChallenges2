using System.Runtime.CompilerServices;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;

namespace CuratedChallenges.ChallengeUtil;

public static class ChallengeIdStore
{
    private static readonly ConditionalWeakTable<ModifierModel, string> _store = new ConditionalWeakTable<ModifierModel, string>();
    
    public static void Set(ModifierModel modifier, string challengeId)
    {
        _store.AddOrUpdate(modifier, challengeId);
    }
    
    public static string Get(ModifierModel modifier)
    {
        return _store.TryGetValue(modifier, out var id) ? id : null;
    }
}