using System.Text.Json.Serialization;
using MegaCrit.Sts2.Core.Saves;

namespace CuratedChallenges.ChallengeUtil.Progression;

[Serializable]
public class ChallengeRunData
{
    [JsonPropertyName("completed_challenges")]
    public Dictionary<string, int> CompletedChallenges { get; set; } = new();
}