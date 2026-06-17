using CuratedChallenges.ChallengeUtil;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace CuratedChallenges.WinConditions;

public abstract class WinCondition : IWinCondition
{
    public abstract bool CheckCondition(IRunState runState, Player player);
    
    public virtual void TriggerVictory(IRunState runState)
    {
        TaskHelper.RunSafely(TriggerVictoryAsync(runState));
    }
    
    protected virtual async Task TriggerVictoryAsync(IRunState runState)
    {
        await Cmd.Wait(0.5f);
        
        await RunManager.Instance.EnterRoom((AbstractRoom)new EventRoom((EventModel)ModelDb.Event<TheArchitect>()));
    }
}