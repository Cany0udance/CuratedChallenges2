using CuratedChallenges.ChallengeUtil;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Runs;

namespace CuratedChallenges.Patches.SpecificChallenges.SplitPersonality;

[HarmonyPatch(typeof(AncientEventModel), "Done")]
public static class AncientEventDoneSeaGlassPatch
{
    static void Postfix(AncientEventModel __instance)
    {
        if (!IsSplitPersonalityActive())
            return;
        
        if (!RunManager.Instance.IsInProgress)
            return;
        
        Player player = __instance.Owner;
        if (player == null)
            return;
        
        // Get all characters except the player's current character
        List<CharacterModel> availableCharacters = ModelDb.AllCharacters
            .Where(c => c.Id != player.Character.Id)
            .ToList();
        
        // Filter out characters the player already has sea glass for
        List<CharacterModel> eligibleCharacters = availableCharacters
            .Where(c => !player.Relics.Any(r => 
                r is SeaGlass seaGlass && seaGlass.CharacterId == c.Id))
            .ToList();
        
        // If no eligible characters remain, do nothing
        if (eligibleCharacters.Count == 0)
            return;
        
        // Pick a random eligible character using the event's RNG
        CharacterModel selectedCharacter = __instance.Rng.NextItem(eligibleCharacters);
        
        // Create and give the seaGlass
        SeaGlass seaGlass = (SeaGlass)ModelDb.Relic<SeaGlass>().ToMutable();
        seaGlass.CharacterId = selectedCharacter.Id;
        
        TaskHelper.RunSafely(RelicCmd.Obtain(seaGlass, player));
    }
    
    private static bool IsSplitPersonalityActive()
    {
        if (!RunManager.Instance.IsInProgress)
            return false;
        
        var runState = RunManager.Instance.DebugOnlyGetState();
        if (runState == null)
            return false;
        
        string challengeId = ChallengeRunTracker.GetChallengeIdFromRun(runState);
        return challengeId == "SPLIT_PERSONALITY" || challengeId?.StartsWith("SPLIT_PERSONALITY") == true;
    }
}