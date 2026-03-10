using System.Reflection;
using CuratedChallenges.ChallengeUtil;
using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.TestSupport;

namespace CuratedChallenges.Patches.SpecificChallenges.FastTrack;

[HarmonyPatch]
public static class FastTrackEnterNextActPatch
{
    private static bool _hasTriggeredVictory = false;
    
    [HarmonyPatch(typeof(RunManager), "EnterNextAct")]
    [HarmonyPrefix]
    static bool EnterNextActPrefix(RunManager __instance, ref Task __result)
    {
        var runState = __instance.DebugOnlyGetState();
        
        if (!IsFastTrackActive(runState))
        {
            _hasTriggeredVictory = false;
            return true;
        }
        
        AbstractRoom currentRoom = runState.CurrentRoom;
        
        // Already handled victory, ignore subsequent calls
        if (_hasTriggeredVictory)
        {
            __result = Task.CompletedTask;
            return false;
        }
        
        // We're in the Architect event, trigger final victory
        if (currentRoom != null && currentRoom.IsVictoryRoom)
        {
            _hasTriggeredVictory = true;
            __result = TriggerFinalVictory(__instance, currentRoom);
            return false;
        }
        
        // Act 2 completed, enter the Architect event
        if (runState.CurrentActIndex == 1)
        {
            __result = EnterVictoryRoom(__instance);
            return false;
        }
        
        return true;
    }
    
    private static async Task EnterVictoryRoom(RunManager manager)
    {
        using (new NetLoadingHandle(manager.NetService))
        {
            if (TestMode.IsOff)
                await NGame.Instance.Transition.RoomFadeOut();
            
            var clearScreensMethod = typeof(RunManager).GetMethod("ClearScreens", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            clearScreensMethod?.Invoke(manager, null);
            
            await manager.EnterRoom((AbstractRoom)new EventRoom((EventModel)ModelDb.Event<TheArchitect>()));
            
            var fadeInMethod = typeof(RunManager).GetMethod("FadeIn", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            await (Task)fadeInMethod?.Invoke(manager, new object[] { true });
        }
    }
    
    private static async Task TriggerFinalVictory(RunManager manager, AbstractRoom currentRoom)
    {
        using (new NetLoadingHandle(manager.NetService))
        {
            ((TheArchitect)((EventRoom)currentRoom).LocalMutableEvent).TriggerVictory();
            
            var onEndedMethod = typeof(RunManager).GetMethod("OnEnded", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            onEndedMethod?.Invoke(manager, new object[] { true });
            
            var killPlayersMethod = typeof(RunManager).GetMethod("GuaranteeKillAllPlayers", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            await (Task)killPlayersMethod?.Invoke(manager, null);
        }
    }
    
    private static bool IsFastTrackActive(RunState runState)
    {
        if (runState == null)
            return false;
    
        string challengeId = ChallengeRunTracker.GetChallengeIdFromRun(runState);
        return challengeId == "FAST_TRACK";
    }
}