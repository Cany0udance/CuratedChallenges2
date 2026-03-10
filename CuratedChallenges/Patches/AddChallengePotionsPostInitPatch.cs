using CuratedChallenges.ChallengeUtil;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;

namespace CuratedChallenges.Patches;

[HarmonyPatch(typeof(RunManager), "InitializeNewRun")]
public static class AddChallengePotionsPostInitPatch
{
    public static void Postfix()
    {
        var runState = RunManager.Instance?.DebugOnlyGetState();
        if (runState == null) return;

        string challengeId = ChallengeRunTracker.GetChallengeIdFromRun(runState);
        if (string.IsNullOrEmpty(challengeId)) return;

        var challengeDef = ChallengeRegistry.GetChallenge(challengeId);
        if (challengeDef == null) return;

        var startingPotions = challengeDef.GetStartingPotions();
        if (startingPotions == null) return;

        foreach (Player player in runState.Players)
        {
            int added = 0;
            foreach (var potionId in startingPotions)
            {
                if (added >= player.MaxPotionCount)
                    break;

                var potionModel = ModelDb.GetById<PotionModel>(potionId);
                var mutablePotion = potionModel.ToMutable();
                player.AddPotionInternal(mutablePotion, -1);
                added++;
            }
        }
    }
}