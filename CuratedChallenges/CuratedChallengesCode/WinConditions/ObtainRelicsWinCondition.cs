using CuratedChallenges.ChallengeUtil;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace CuratedChallenges.WinConditions;

public class ObtainRelicWinCondition : WinCondition
{
    private readonly ModelId _relicId;
    private readonly int _requiredCount;
    
    public ObtainRelicWinCondition(ModelId relicId, int count = 1)
    {
        _relicId = relicId;
        _requiredCount = count;
    }
    
    public override bool CheckCondition(IRunState runState, Player player)
    {
        int count = player.Relics.Count(r => r.Id == _relicId);
        return count >= _requiredCount;
    }
}