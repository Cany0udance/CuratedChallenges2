using System.Reflection.Emit;
using CuratedChallenges.ChallengeUtil;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Runs;

namespace CuratedChallenges.Patches;

[HarmonyPatch(typeof(Neow), "GenerateInitialOptions")]
public static class NeowChallengePatch
{
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var codes = instructions.ToList();
        bool patched = false;
        
        for (int i = 0; i < codes.Count - 1; i++)
        {
            bool isModifiersCall = codes[i].opcode == OpCodes.Callvirt && 
                                   codes[i].operand?.ToString()?.Contains("get_Modifiers") == true;
            bool isCountCall = codes[i + 1].opcode == OpCodes.Callvirt && 
                               codes[i + 1].operand?.ToString()?.Contains("get_Count") == true;
            
            if (isModifiersCall && isCountCall && !patched)
            {
                codes[i] = new CodeInstruction(OpCodes.Call, 
                    AccessTools.Method(typeof(NeowChallengePatch), nameof(GetEffectiveModifierCount)));
                codes[i + 1] = new CodeInstruction(OpCodes.Nop);
                patched = true;
            }
        }
        
        return codes;
    }
    
    public static int GetEffectiveModifierCount(IRunState runState)
    {
        var modifiers = runState.Modifiers;
        
        if (modifiers.Count == 1 && 
            modifiers[0] is ChallengeModifier challenge && 
            challenge.AllowNeowBonuses)
        {
            return 0;
        }
        
        return modifiers.Count;
    }
}