using System.Reflection;
using CuratedChallenges.ChallengeUtil;
using HarmonyLib;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.TestSupport;

namespace CuratedChallenges.Patches.SpecificChallenges.FastTrack;

[HarmonyPatch(typeof(RunManager), "EnterAct", MethodType.Normal)]
public static class FastTrackEnterActPatch
{
    private static MethodInfo _clearScreensMethod;
    private static MethodInfo _exitCurrentRoomsMethod;
    
    static FastTrackEnterActPatch()
    {
        _clearScreensMethod = typeof(RunManager).GetMethod("ClearScreens", 
            BindingFlags.NonPublic | BindingFlags.Instance);
        _exitCurrentRoomsMethod = typeof(RunManager).GetMethod("ExitCurrentRooms", 
            BindingFlags.NonPublic | BindingFlags.Instance);
    }
    
    static bool Prefix(ref Task __result, RunManager __instance, int currentActIndex, bool doTransition)
    {
        var runState = __instance.DebugOnlyGetState();
        if (!IsFastTrackActive(runState))
            return true; // Run original method
        
        // If entering Act 1, redirect to Act 2
        if (currentActIndex == 0)
        {
            __result = EnterActTwo(__instance, runState, doTransition);
            return false; // Skip original method
        }
        
        return true; // Run original method for other acts
    }
    
    private static async Task EnterActTwo(RunManager manager, RunState runState, bool doTransition)
    {
    
        if (TestMode.IsOff)
            await NGame.Instance.Transition.RoomFadeOut();
    
        using (new NetLoadingHandle(manager.NetService))
        {
            _clearScreensMethod.Invoke(manager, null);
            await (Task)_exitCurrentRoomsMethod.Invoke(manager, null);
            await manager.SetActInternal(1); // Set to Act 2
        
            // Auto-enter the starting map point (like Neow behavior)
            if (NRun.Instance != null)
                NMapScreen.Instance?.InitMarker(runState.Map.StartingMapPoint.coord);
            await manager.EnterMapCoord(runState.Map.StartingMapPoint.coord);
            NMapScreen.Instance?.RefreshAllMapPointVotes();
        
            await Hook.AfterActEntered((IRunState)runState);
        
            // Manually update the boss icon to ensure it shows Act 2 boss
            NRun.Instance?.GlobalUi.TopBar.BossIcon.Initialize(runState);
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