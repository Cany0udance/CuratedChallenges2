using HarmonyLib;
using MegaCrit.Sts2.Core.Saves;

namespace CuratedChallenges.ChallengeUtil.Progression;

[HarmonyPatch(typeof(SaveManager), nameof(SaveManager.SwitchProfileId))]
public class SaveManagerSwitchProfilePatch
{
    static void Postfix(int profileId)
    {
        ChallengeDataManager.ClearCache();
    }
}