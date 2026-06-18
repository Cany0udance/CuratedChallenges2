using System.Text.Json.Serialization;

namespace CuratedChallenges.ChallengeUtil.Progression;

[Serializable]
public class ChallengeRunData
{
    [JsonPropertyName("completed_challenges")]
    public Dictionary<string, int> CompletedChallenges { get; set; } = new();
}