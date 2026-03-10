using CuratedChallenges.ChallengeUtil;
using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace CuratedChallenges.Patches;

[HarmonyPatch(typeof(ModifierModel), nameof(ModifierModel.ToSerializable))]
public class ModifierModelToSerializableInjectPatch
{
    static void Postfix(ModifierModel __instance, SerializableModifier __result)
    {
        if (__instance is ChallengeModifier cm)
        {
            if (__result.Props == null)
                __result.Props = new SavedProperties();
            
            if (__result.Props.strings == null)
                __result.Props.strings = new List<SavedProperties.SavedProperty<string>>();
            
            __result.Props.strings.Add(new SavedProperties.SavedProperty<string>("ChallengeId", cm.ChallengeId));
        }
    }
}