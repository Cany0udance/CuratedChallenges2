using CuratedChallenges.ChallengeUtil;
using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;

namespace CuratedChallenges.Patches;

[HarmonyPatch(typeof(RunManager), nameof(RunManager.GenerateRooms))]
public static class RunManagerGenerateRoomsPatch
{
    static void Prefix(RunManager __instance)
    {
        var stateField = AccessTools.Property(typeof(RunManager), "State");
        var runState = (RunState)stateField.GetValue(__instance);
        
        if (runState == null || !ChallengeRunTracker.IsChallengeRun(runState)) return;
        
        var challengeMod = runState.Modifiers.OfType<ChallengeModifier>().FirstOrDefault();
        if (challengeMod == null) return;
        
        BlacklistedEventsCache.Set(challengeMod.Challenge.GetBlacklistedEvents());
    }
    
    static void Postfix()
    {
        BlacklistedEventsCache.Clear();
    }
}

[HarmonyPatch(typeof(ActModel), nameof(ActModel.GenerateRooms))]
public static class ActModelGenerateRoomsPatch
{
    static void Postfix(ActModel __instance)
    {
        var blacklistedEventIds = BlacklistedEventsCache.Get();
        if (blacklistedEventIds.Count == 0) return;
        
        var roomsField = AccessTools.Field(typeof(ActModel), "_rooms");
        var rooms = roomsField.GetValue(__instance);
        var eventsField = AccessTools.Field(rooms.GetType(), "events");
        var eventsList = (List<EventModel>)eventsField.GetValue(rooms);
        
        int removedCount = eventsList.RemoveAll(e => blacklistedEventIds.Contains(e.Id.Entry));
        
        if (removedCount > 0)
        {
           // Log.Info($"[ActModelGenerateRoomsPatch] Removed {removedCount} blacklisted events from act {__instance.Id.Entry}");
        }
    }
}
