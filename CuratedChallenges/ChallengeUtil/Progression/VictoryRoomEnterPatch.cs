using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace CuratedChallenges.ChallengeUtil.Progression;

[HarmonyPatch(typeof(TheArchitect), nameof(TheArchitect.TriggerVictory))]
public class VictoryTriggeredPatch
{
    static void Postfix(TheArchitect __instance)
    {
        var runState = RunManager.Instance?.DebugOnlyGetState();
        if (runState == null)
        {
            return;
        }
        
        SaveChallengeCompletionIfApplicable(runState);
    }
    
    private static void SaveChallengeCompletionIfApplicable(IRunState runState)
    {
        string challengeId = ChallengeRunTracker.GetChallengeIdFromRun(runState);
        
        if (challengeId == null)
        {
            return;
        }
        
        var challengeDef = ChallengeRegistry.GetChallenge(challengeId);
        if (challengeDef == null)
        {
            return;
        }
        
        string storageKey = challengeId;
        
        if (challengeDef.IsShared)
        {
            var character = runState.Players[0].Character;
            storageKey = $"{challengeId}_{character.Id.Entry}";
        }
        
        int ascension = runState.AscensionLevel;
        
        var data = ChallengeDataManager.GetData();
        if (!data.CompletedChallenges.TryGetValue(storageKey, out int highestAscension) 
            || ascension > highestAscension)
        {
            data.CompletedChallenges[storageKey] = ascension;
            ChallengeDataManager.SaveData();
        }
    }
}