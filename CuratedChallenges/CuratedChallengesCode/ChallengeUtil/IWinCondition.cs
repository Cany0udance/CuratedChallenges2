using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Runs;

namespace CuratedChallenges.ChallengeUtil;

public interface IWinCondition
{
    bool CheckCondition(IRunState runState, Player player);
    void TriggerVictory(IRunState runState);
}
