namespace CuratedChallenges.ChallengeUtil;

public static class VakuuAutoEndState
{
    public static bool IsEnabled { get; set; }
    
    public static void Reset()
    {
        IsEnabled = false;
    }
}