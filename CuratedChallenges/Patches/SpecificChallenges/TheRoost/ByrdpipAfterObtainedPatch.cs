using System.Reflection;
using CuratedChallenges.ChallengeUtil;
using CuratedChallenges.Modifiers;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;

namespace CuratedChallenges.Patches.SpecificChallenges.TheRoost;

[HarmonyPatch(typeof(Byrdpip), nameof(Byrdpip.AfterObtained))]
public static class ByrdpipAfterObtainedPatch
{
    static bool Prefix(Byrdpip __instance, ref Task __result)
    {
        if (!IsTheRoostActive())
            return true; // Use original behavior
        
        // Run custom logic for The Roost
        __result = TransformSingleEgg(__instance);
        return false; // Skip original method
    }
    
    private static async Task TransformSingleEgg(Byrdpip byrdpip)
    {
        byrdpip.Skin = new Rng((uint)(byrdpip.Owner.NetId + (ulong)byrdpip.Owner.RunState.Rng.Seed))
            .NextItem<string>((IEnumerable<string>)Byrdpip.SkinOptions);
        
        // Find first egg in deck
        var egg = PileType.Deck.GetPile(byrdpip.Owner).Cards
            .FirstOrDefault(c => c is ByrdonisEgg);
        
        if (egg != null)
        {
            await CardCmd.TransformTo<ByrdSwoop>(egg, CardPreviewStyle.HorizontalLayout);
        }
        
        // Summon pet if in combat
        if (CombatManager.Instance.IsInProgress)
        {
            await PlayerCmd.AddPet<MegaCrit.Sts2.Core.Models.Monsters.Byrdpip>(byrdpip.Owner);
        }
    }
    
    private static bool IsTheRoostActive()
    {
        if (!RunManager.Instance.IsInProgress)
            return false;
        
        var runState = RunManager.Instance.DebugOnlyGetState();
        if (runState == null)
            return false;
        
        string challengeId = ChallengeRunTracker.GetChallengeIdFromRun(runState);
        return challengeId == "THE_ROOST" || challengeId?.StartsWith("THE_ROOST") == true;
    }
}