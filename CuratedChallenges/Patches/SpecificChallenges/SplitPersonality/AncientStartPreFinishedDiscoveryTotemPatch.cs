using CuratedChallenges.ChallengeUtil;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Runs;

namespace CuratedChallenges.Patches.SpecificChallenges.SplitPersonality;

[HarmonyPatch(typeof(AncientEventModel), "StartPreFinished")]
public static class AncientStartPreFinishedSeaGlassPatch
{
    static void Postfix(AncientEventModel __instance)
    {
        
        if (!(__instance is Neow))
        {
            return;
        }
        
        if (!IsSplitPersonalityActive())
        {
            return;
        }
        
        if (!RunManager.Instance.IsInProgress)
        {
            return;
        }
        
        Player player = __instance.Owner;
        if (player == null)
        {
            return;
        }
        
        GiveSeaGlass(__instance, player);
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
    
    private static void GiveSeaGlass(AncientEventModel ancientEvent, Player player)
    {
        
        List<CharacterModel> availableCharacters = ModelDb.AllCharacters
            .Where(c => c.Id != player.Character.Id)
            .ToList();
        
        List<CharacterModel> eligibleCharacters = availableCharacters
            .Where(c => !player.Relics.Any(r => 
                r is SeaGlass seaGlass && seaGlass.CharacterId == c.Id))
            .ToList();
        
        if (eligibleCharacters.Count == 0)
        {
            return;
        }
        
        CharacterModel selectedCharacter = ancientEvent.Rng.NextItem(eligibleCharacters);
        
        SeaGlass seaGlass = (SeaGlass)ModelDb.Relic<SeaGlass>().ToMutable();
        seaGlass.CharacterId = selectedCharacter.Id;
        
        TaskHelper.RunSafely(RelicCmd.Obtain(seaGlass, player));
    }
}