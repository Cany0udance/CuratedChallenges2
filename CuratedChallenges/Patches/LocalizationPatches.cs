using System.Reflection;
using CuratedChallenges.ChallengeUtil;
using HarmonyLib;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;

namespace CuratedChallenges.MainMenu;

[HarmonyPatch(typeof(LocManager), "SetLanguage")]
public class ChallengesLocTablePatch
{
    public static void Postfix(LocManager __instance)
    {
        var tablesField = typeof(LocManager).GetField("_tables", BindingFlags.NonPublic | BindingFlags.Instance);
        var tables = (Dictionary<string, LocTable>)tablesField.GetValue(__instance);
        
        if (!tables.ContainsKey("challenges"))
        {
            tables["challenges"] = new LocTable("challenges", new Dictionary<string, string>());
        }
    }
}

[HarmonyPatch(typeof(LocTable), "GetRawText")]
public class LocTableGetRawTextPatch
{
    private static readonly FieldInfo NameField = typeof(LocTable).GetField("_name", BindingFlags.NonPublic | BindingFlags.Instance);
    private static readonly FieldInfo TranslationsField = typeof(LocTable).GetField("_translations", BindingFlags.NonPublic | BindingFlags.Instance);
    
    static bool Prefix(LocTable __instance, string key, ref string __result)
    {
        var tableName = NameField?.GetValue(__instance) as string;
        if (tableName != "challenges") return true;
        
        var data = TranslationsField?.GetValue(__instance) as Dictionary<string, string>;
        if (data == null || data.ContainsKey(key)) return true;
        
        string challengeId;
        bool isTitle;
        
        if (key.EndsWith(".title"))
        {
            challengeId = key.Replace(".title", "");
            isTitle = true;
        }
        else if (key.EndsWith(".description"))
        {
            challengeId = key.Replace(".description", "");
            isTitle = false;
        }
        else
        {
            return true;
        }
        
        var challenge = ChallengeRegistry.GetChallenge(challengeId);
        if (challenge == null) return true;
        
        var value = isTitle ? challenge.Name : BuildChallengeDescription(challenge);
        data[key] = value;
        __result = value;
        return false;
    }
    
    private static string BuildChallengeDescription(ChallengeDefinition challenge)
    {
        var textProcessor = new ChallengeTopPanelTextColorProcessor();
        var lines = new List<string>();
        
        if (!string.IsNullOrEmpty(challenge.SpecialRules))
        {
            lines.Add("[gold]Special Rules:[/gold]");
            lines.Add("");
            var rulesLines = challenge.SpecialRules.Split(new[] { " \n " }, StringSplitOptions.RemoveEmptyEntries);
            
            for (int i = 0; i < rulesLines.Length; i++)
            {
                lines.Add("• " + textProcessor.AutoColorText(rulesLines[i].Trim()));
                if (rulesLines.Length >= 2 && i < rulesLines.Length - 1)
                    lines.Add("");
            }
        }
        
        if (!string.IsNullOrEmpty(challenge.WinConditions))
        {
            if (lines.Count > 0) lines.Add("");
            lines.Add("[gold]Win Conditions:[/gold]");
            lines.Add("");
            var winLines = challenge.WinConditions.Split(new[] { " \n " }, StringSplitOptions.RemoveEmptyEntries);
            
            for (int i = 0; i < winLines.Length; i++)
            {
                lines.Add("• " + textProcessor.AutoColorText(winLines[i].Trim()));
                if (winLines.Length >= 2 && i < winLines.Length - 1)
                    lines.Add("");
            }
        }
        
        return string.Join("\n", lines);
    }
}