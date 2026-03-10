using System.Text.RegularExpressions;

namespace CuratedChallenges.ChallengeUtil;

public class ChallengeTopPanelTextColorProcessor
{
    // Static regex patterns - compiled once
    private static readonly Regex NumbersRegex = new Regex(@"(?<!\[)(?<!\w)(\d+)(?!\w)(?!\])", RegexOptions.Compiled);
    private static readonly Regex OpenTagRegex = new Regex(@"\[(gold|red|green|blue|purple)\]", RegexOptions.Compiled);
    private static readonly Regex CloseTagRegex = new Regex(@"\[/(gold|red|green|blue|purple)\]", RegexOptions.Compiled);
    
    // Pre-sorted term lists with cached regex patterns
    private static readonly List<(string term, Regex regex)> RedTerms;
    private static readonly List<(string term, Regex regex)> GoldTerms;
    private static readonly List<(string term, Regex regex)> PurpleTerms;
    
    static ChallengeTopPanelTextColorProcessor()
    {
        RedTerms = BuildTermList(new[] { "Curse", "Curses" });
        
        PurpleTerms = BuildTermList(new[] { "Slither", "Galvanic" });
        
        GoldTerms = BuildTermList(new[]
        {
            // Keywords
            "Vulnerable", "Weak", "Frail", "Strength", "Dexterity", "Rest Site", "Act", "Enchant", "Enchanted", "Relic", "Relics",
            // Cards
            "Wound", "Infection", "Dazed", "Slimed", "Burn", "Bombardment",
            // Relics
            "Byrdpip", "Byrdpips", "Byrdonis Egg", "Wax Choker", "Runic Pyramid", "Discovery Totem",
            // Potions
            "Gambler's Brew", "Gamblers Brew", "Cure All"
        });
    }
    
    private static List<(string term, Regex regex)> BuildTermList(string[] terms)
    {
        return terms
            .OrderByDescending(t => t.Length)
            .Select(t => (t, new Regex($@"(?<!\[)\b({Regex.Escape(t)})\b(?!\])", RegexOptions.Compiled)))
            .ToList();
    }
    
    public string AutoColorText(string text)
    {
        text = AutoColorNumbers(text);
        text = AutoColorTerms(text, RedTerms, "red");
        text = AutoColorTerms(text, GoldTerms, "gold");
        text = AutoColorTerms(text, PurpleTerms, "purple");
        return text;
    }
    
    private string AutoColorNumbers(string text)
    {
        return NumbersRegex.Replace(text, match =>
        {
            if (IsInsideColorTag(text, match.Index))
                return match.Value;
            return $"[blue]{match.Value}[/blue]";
        });
    }
    
    private string AutoColorTerms(string text, List<(string term, Regex regex)> terms, string color)
    {
        foreach (var (_, regex) in terms)
        {
            text = regex.Replace(text, match =>
            {
                if (IsInsideColorTag(text, match.Index))
                    return match.Value;
                return $"[{color}]{match.Value}[/{color}]";
            });
        }
        return text;
    }
    
    private bool IsInsideColorTag(string text, int position)
    {
        var beforeText = text.Substring(0, position);
        var openTags = OpenTagRegex.Matches(beforeText).Count;
        var closeTags = CloseTagRegex.Matches(beforeText).Count;
        return openTags > closeTags;
    }
}