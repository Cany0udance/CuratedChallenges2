namespace CuratedChallenges.ChallengeUtil;

public static class BlacklistedEventsCache
{
    private static HashSet<string> _blacklistedEvents = new HashSet<string>();
    
    public static void Set(IEnumerable<string> events)
    {
        _blacklistedEvents = events.ToHashSet();
    }
    
    public static HashSet<string> Get() => _blacklistedEvents;
    
    public static void Clear() => _blacklistedEvents.Clear();
}