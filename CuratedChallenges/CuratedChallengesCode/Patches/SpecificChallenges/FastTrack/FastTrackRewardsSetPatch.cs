using CuratedChallenges.ChallengeUtil;
using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace CuratedChallenges.Patches.SpecificChallenges.FastTrack;

[HarmonyPatch(typeof(RewardsSet), "WithRewardsFromRoom", MethodType.Normal)]
public static class FastTrackRewardsSetPatch
{
    static bool Prefix(RewardsSet __instance, AbstractRoom room, ref RewardsSet __result)
    {
        var player = __instance.Player;
        
        if (!IsFastTrackActive(player?.RunState as RunState))
            return true; // Run original method
        
        // Set the Room property using reflection since it's private set
        var roomProperty = typeof(RewardsSet).GetProperty("Room");
        roomProperty.SetValue(__instance, room);
        
        // If it's a boss room in Act 2 (index 1), skip rewards
        if (room.RoomType == RoomType.Boss && player.RunState.CurrentActIndex == 1)
        {
            __result = __instance;
            return false; // Skip original method
        }
        
        return true; // Run original method for other cases
    }
    
    private static bool IsFastTrackActive(RunState runState)
    {
        if (runState == null)
            return false;
    
        string challengeId = ChallengeRunTracker.GetChallengeIdFromRun(runState);
        return challengeId == "FAST_TRACK";
    }
}