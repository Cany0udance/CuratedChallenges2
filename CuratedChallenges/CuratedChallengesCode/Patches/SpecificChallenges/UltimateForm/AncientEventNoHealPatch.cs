using CuratedChallenges.ChallengeUtil;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Runs;

namespace CuratedChallenges.Patches.SpecificChallenges.UltimateForm;
[HarmonyPatch(typeof(AncientEventModel), "BeforeEventStarted", MethodType.Normal)]
public static class AncientEventNoHealPatch
{
    static bool Prefix(AncientEventModel __instance, ref Task __result)
    {
        // Allow Neow to heal normally
        if (__instance is Neow)
            return true;
        
        if (!IsUltimateFormActive())
            return true; // Use original behavior
        
        // Skip healing entirely for non-Neow ancients in ULTIMATE_FORM
        // Set HealedAmount to 0
        var property = typeof(AncientEventModel).GetProperty("HealedAmount", 
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        property?.SetValue(__instance, 0);
        
        // Return a completed task to prevent null reference
        __result = Task.CompletedTask;
        return false; // Skip the original method
    }
    
    private static bool IsUltimateFormActive()
    {
        if (!RunManager.Instance.IsInProgress)
            return false;
    
        var runState = RunManager.Instance.DebugOnlyGetState();
        if (runState == null)
            return false;
    
        string challengeId = ChallengeRunTracker.GetChallengeIdFromRun(runState);
        return challengeId == "ULTIMATE_FORM";
    }
}