using System.Reflection;
using CuratedChallenges.ChallengeUtil;
using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace CuratedChallenges.Patches;

[HarmonyPatch(typeof(ModifierModel), nameof(ModifierModel.FromSerializable))]
public class ModifierModelFromSerializablePatch
{
    static bool Prefix(SerializableModifier serializable, ref ModifierModel __result)
    {
        // Check if this is a ChallengeModifier by looking for ChallengeId in Props
        if (serializable.Props?.strings != null)
        {
            var challengeIdProp = serializable.Props.strings
                .FirstOrDefault(s => s.name == "ChallengeId");
            
            if (challengeIdProp.name != null && !string.IsNullOrEmpty(challengeIdProp.value))
            {
                
                try
                {
                    // Get the challenge definition
                    var challenge = ChallengeRegistry.GetChallenge(challengeIdProp.value);
                    
                    if (challenge != null)
                    {
                        // Create a new ChallengeModifier
                        __result = challenge.CreateModifier();
                        
                        // Return false to skip the original method
                        return false;
                    }

                }
                catch (Exception e)
                {

                }
            }
        }
        return true;
    }
}