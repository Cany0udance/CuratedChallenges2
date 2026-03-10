using System.Reflection;
using CuratedChallenges.ChallengeUtil;
using HarmonyLib;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace CuratedChallenges.Patches;

[HarmonyPatch(typeof(RunManager), nameof(RunManager.EnterNextAct))]
public static class EnterNextActPatch
{
    static bool Prefix(RunManager __instance, ref Task __result)
    {
        var runState = __instance.DebugOnlyGetState();
        if (runState == null) return true;
        
        var challengeMod = runState.Modifiers.OfType<ChallengeModifier>().FirstOrDefault();
        if (challengeMod == null) return true;
        
        AbstractRoom currentRoom = runState.CurrentRoom;
        
        if (currentRoom != null && currentRoom.IsVictoryRoom)
        {
            __result = HandleVictory(__instance, currentRoom);
            return false;
        }
        
        return true;
    }
    
    private static async Task HandleVictory(RunManager manager, AbstractRoom currentRoom)
    {
        using (new NetLoadingHandle(manager.NetService))
        {
            ((TheArchitect)((EventRoom)currentRoom).LocalMutableEvent).TriggerVictory();
            manager.OnEnded(true);
            
            var killPlayersMethod = typeof(RunManager).GetMethod("GuaranteeKillAllPlayers", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            await (Task)killPlayersMethod?.Invoke(manager, null);
        }
    }
}