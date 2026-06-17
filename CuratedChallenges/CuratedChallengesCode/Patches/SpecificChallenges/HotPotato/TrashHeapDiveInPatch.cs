using System.Reflection.Emit;
using CuratedChallenges.ChallengeUtil;
using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Runs;

namespace CuratedChallenges.Patches.SpecificChallenges.HotPotato;

[HarmonyPatch(typeof(TrashHeap), "DiveIn")]
public static class TrashHeapDiveInPatch
{
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
    {
        var codes = new List<CodeInstruction>(instructions);
        
        // Find the instruction that calls Rng.NextItem<RelicModel>
        for (int i = 0; i < codes.Count; i++)
        {
            if (codes[i].opcode == OpCodes.Callvirt && 
                codes[i].operand?.ToString().Contains("NextItem") == true)
            {
                // Insert our filtering logic before NextItem is called
                // The _relics array should already be on the stack
                codes.Insert(i, new CodeInstruction(OpCodes.Ldarg_0)); // Load 'this' (TrashHeap)
                codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, 
                    AccessTools.Method(typeof(TrashHeapDiveInPatch), nameof(FilterRelics))));
                break;
            }
        }
        
        return codes;
    }
    
    static IEnumerable<RelicModel> FilterRelics(IEnumerable<RelicModel> relics, TrashHeap trashHeap)
    {
        if (!RunManager.Instance.IsInProgress)
            return relics;
            
        var runState = RunManager.Instance.DebugOnlyGetState();
        if (runState == null || !ChallengeRunTracker.IsChallengeRun(runState))
            return relics;
        
        var player = trashHeap.Owner;
        var ownedRelicIds = player.Relics.Select(r => r.Id).ToHashSet();
        
        var filtered = relics.Where(r => !ownedRelicIds.Contains(r.Id)).ToList();
        
        if (filtered.Count == 0)
        {
            return relics;
        }
        return filtered;
    }
}