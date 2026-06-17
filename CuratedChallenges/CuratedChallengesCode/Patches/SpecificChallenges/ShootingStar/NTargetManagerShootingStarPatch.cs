using System.Reflection;
using CuratedChallenges.ChallengeUtil;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Runs;

namespace CuratedChallenges.Patches.SpecificChallenges.ShootingStar;

[HarmonyPatch(typeof(NTargetManager))]
public static class NTargetManagerShootingStarPatch
{
    private static readonly FieldInfo ValidTargetsTypeField = 
        AccessTools.Field(typeof(NTargetManager), "_validTargetsType");
    
    [HarmonyPatch("AllowedToTargetCreature")]
    [HarmonyPrefix]
    static bool AllowedToTargetCreaturePrefix(
        NTargetManager __instance, 
        Creature creature, 
        ref bool __result)
    {
        if (!IsShootingStarActive())
            return true; // Run original method
        
        // Check if we're currently targeting with Bombardment
        var validTargetsType = (TargetType)ValidTargetsTypeField.GetValue(__instance);
        
        if (validTargetsType != TargetType.AnyEnemy)
        {
            return true; // Not targeting enemies, run original
        }
        
        // Allow targeting both enemies and the player
        if (creature.Side == CombatSide.Enemy)
        {
            __result = true;
            return false; // Skip original
        }
        
        if (creature.IsPlayer && !creature.IsDead && LocalContext.IsMe(creature.Player))
        {
            __result = true;
            return false; // Skip original
        }
        __result = false;
        return false; // Skip original
    }
    
    private static bool IsShootingStarActive()
    {
        if (!RunManager.Instance.IsInProgress)
            return false;
        
        var runState = RunManager.Instance.DebugOnlyGetState();
        if (runState == null)
            return false;
        
        string challengeId = ChallengeRunTracker.GetChallengeIdFromRun(runState);
        bool isActive = challengeId == "SHOOTING_STAR" || challengeId?.StartsWith("SHOOTING_STAR") == true;
        
        return isActive;
    }
}