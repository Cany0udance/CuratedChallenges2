using System.Reflection;
using CuratedChallenges.Panels;
using CuratedChallenges.Screens;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Nodes.Screens.RunHistoryScreen;

namespace CuratedChallenges.Patches;

[HarmonyPatch(typeof(NDeckHistoryEntry), "Reload")]
public class NDeckHistoryEntryReloadPatch
{
    static void Postfix(NDeckHistoryEntry __instance)
    {
        // Check if this entry is part of the challenges screen
        if (!IsInChallengesScreen(__instance))
            return;
        
        // Get the private _titleLabel field
        var titleLabelField = typeof(NDeckHistoryEntry).GetField("_titleLabel", 
            BindingFlags.NonPublic | BindingFlags.Instance);
        var titleLabel = titleLabelField?.GetValue(__instance) as MegaRichTextLabel;
        
        var amountField = typeof(NDeckHistoryEntry).GetField("_amount",
            BindingFlags.NonPublic | BindingFlags.Instance);
        int amount = amountField != null ? (int)amountField.GetValue(__instance) : 1;
        
        if (titleLabel != null && __instance.Card != null)
        {
            string fullTitle = __instance.Card.Title;
            if (amount > 1)
                fullTitle = $"{amount}x {fullTitle}";
            
            // Preserve color formatting if card is upgraded or enchanted
            bool isUpgraded = __instance.Card.CurrentUpgradeLevel >= 1;
            bool isEnchanted = __instance.Card.Enchantment != null;
            
            if (isEnchanted)
                titleLabel.Text = $"[purple]{fullTitle}[/purple]";
            else if (isUpgraded)
                titleLabel.Text = $"[green]{fullTitle}[/green]";
            else
                titleLabel.Text = fullTitle;
        }
    }
    
    private static bool IsInChallengesScreen(Node node)
    {
        Node current = node;
        while (current != null)
        {
            if (current is NChallengesScreen || current is NChallengeDetailsPanel)
                return true;
            current = current.GetParent();
        }
        return false;
    }
}