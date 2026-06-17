using System.Reflection;
using CuratedChallenges.ChallengeUtil;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;

namespace CuratedChallenges.Patches;

[HarmonyPatch(typeof(Player), "PopulateStartingRelics")]
public class PopulateStartingRelicsPatch
{
    static bool Prefix(Player __instance)
    {
       
        if (ChallengeModifier.IsStartingChallengeRun)
        {
            return false;
        }
       
        return true;
    }
}