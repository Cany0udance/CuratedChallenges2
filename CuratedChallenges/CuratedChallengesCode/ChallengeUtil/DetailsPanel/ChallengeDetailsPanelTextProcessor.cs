using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Afflictions;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Models.Potions;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.Relics;

namespace CuratedChallenges.ChallengeUtil.DetailsPanel;

using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

/// <summary>
/// Processes challenge description text with support for color tags and keyword/card name detection.
/// Parses text containing formatting like [gold]text[/gold] and automatically identifies game keywords
/// (e.g., Vulnerable, Weak) and card names (e.g., Wound, Burn) to create interactive label hierarchies.
/// Returns Godot Control hierarchies (VBoxContainer, HFlowContainer) with properly formatted labels
/// that can have tooltip handlers attached by the parent panel.
/// </summary>
public class ChallengeDetailsPanelTextProcessor
{
   private Dictionary<string, IHoverTip> _keywordTooltips;
   private Dictionary<string, CardModel> _cardNameTooltips;
   private Dictionary<string, IEnumerable<IHoverTip>> _relicNameTooltips;
   private Dictionary<string, Func<IEnumerable<IHoverTip>>> _enchantmentNameTooltips;
   private Dictionary<string, IEnumerable<IHoverTip>> _potionNameTooltips;
   private ChallengeDetailsPanelLabelFactory _labelFactory;
 
   public ChallengeDetailsPanelTextProcessor(ChallengeDetailsPanelLabelFactory labelFactory)
   {
       _labelFactory = labelFactory;
       InitializeTooltips();
   }
 
   private void InitializeTooltips()
   {
       _keywordTooltips = new Dictionary<string, IHoverTip>
       {
           { "Vulnerable", HoverTipFactory.FromPower<VulnerablePower>() },
           { "Weak", HoverTipFactory.FromPower<WeakPower>() },
           { "Frail", HoverTipFactory.FromPower<FrailPower>() },
           { "Strength", HoverTipFactory.FromPower<StrengthPower>() },
           { "Dexterity", HoverTipFactory.FromPower<DexterityPower>() },
           { "Exhaust", HoverTipFactory.FromKeyword(CardKeyword.Exhaust) },
           { "Exhausted", HoverTipFactory.FromKeyword(CardKeyword.Exhaust) },
       };
     
       _cardNameTooltips = new Dictionary<string, CardModel>
       {
           { "Wound", ModelDb.Card<Wound>() },
           { "Infection", ModelDb.Card<Infection>() },
           { "Dazed", ModelDb.Card<Dazed>() },
           { "Slimed", ModelDb.Card<Slimed>() },
           { "Burn", ModelDb.Card<Burn>() },
           { "Bombardment", ModelDb.Card<Bombardment>() },
       };
      
       _relicNameTooltips = new Dictionary<string, IEnumerable<IHoverTip>>
       {
            { "Byrdpip", HoverTipFactory.FromRelic<Byrdpip>() },
            { "Byrdpips", HoverTipFactory.FromRelic<Byrdpip>() },
            { "Toy Box", HoverTipFactory.FromRelic<ToyBox>() },
            { "Runic Pyramid", HoverTipFactory.FromRelic<RunicPyramid>() },
            { "Sea Glass", HoverTipFactory.FromRelic<SeaGlass>() },
       };
       
       _enchantmentNameTooltips = new Dictionary<string, Func<IEnumerable<IHoverTip>>>
       {
           { "Slither", () => HoverTipFactory.FromEnchantment<Slither>(1) },
           { "Galvanic", () => HoverTipFactory.FromAffliction<Galvanized>(6) }
       };
       
       _potionNameTooltips = new Dictionary<string, IEnumerable<IHoverTip>>
       {
           { "Gambler's Brew", new[] { HoverTipFactory.FromPotion(ModelDb.Potion<GamblersBrew>()) } },
           { "Gamblers Brew", new[] { HoverTipFactory.FromPotion(ModelDb.Potion<GamblersBrew>()) } },
           { "Cure All", new[] { HoverTipFactory.FromPotion(ModelDb.Potion<CureAll>()) } }
       };
   }
  
  public Control CreateRichTextLabelWithKeywords(string text, float contentWidth)
{
   text = AutoColorText(text);
  
   var container = new VBoxContainer();
   container.AddThemeConstantOverride("separation", 5);
  
   string[] lines = text.Split('\n');
  
   foreach (string line in lines)
   {
       var lineContainer = CreateFlowContainerWithKeywords(line, contentWidth);
       container.AddChild(lineContainer);
   }
  
   return container;
}

private string AutoColorText(string text)
{
   text = AutoColorNumbers(text);
   text = AutoColorSpecialTerms(text);
   return text;
}

private string AutoColorNumbers(string text)
{
   var regex = new System.Text.RegularExpressions.Regex(@"(?<!\[(?:gold|red|green|blue|purple)\])(?<!\w)(\d+)(?!\w)(?!\])");
  
   return regex.Replace(text, match =>
   {
       int matchIndex = match.Index;
       if (IsInsideColorTag(text, matchIndex))
           return match.Value;
      
       return $"[blue]{match.Value}[/blue]";
   });
}

private string AutoColorSpecialTerms(string text)
{
    var goldTerms = new HashSet<string>();
   
    foreach (var keyword in _keywordTooltips.Keys)
        goldTerms.Add(keyword);
   
    foreach (var cardName in _cardNameTooltips.Keys)
        goldTerms.Add(cardName);
   
    foreach (var relicName in _relicNameTooltips.Keys)
        goldTerms.Add(relicName);
    
    foreach (var potionName in _potionNameTooltips.Keys)
        goldTerms.Add(potionName);
   
    goldTerms.UnionWith(GetCustomGoldTerms());
   
    var purpleTerms = new HashSet<string>();
    foreach (var enchantmentName in _enchantmentNameTooltips.Keys)
        purpleTerms.Add(enchantmentName);
    
    var redTerms = GetCustomRedTerms();
   
    var sortedGoldTerms = goldTerms.OrderByDescending(t => t.Length).ToList();
    var sortedPurpleTerms = purpleTerms.OrderByDescending(t => t.Length).ToList();
    var sortedRedTerms = redTerms.OrderByDescending(t => t.Length).ToList();
   
    // Color red terms first
    foreach (var term in sortedRedTerms)
    {
        var pattern = $@"(?<!\[(?:gold|red|green|blue|purple)\])(?<!\[/)\b({System.Text.RegularExpressions.Regex.Escape(term)})\b(?!\])";
        var regex = new System.Text.RegularExpressions.Regex(pattern);
       
        text = regex.Replace(text, match =>
        {
            if (IsInsideColorTag(text, match.Index))
                return match.Value;
           
            return $"[red]{match.Value}[/red]";
        });
    }
   
    foreach (var term in sortedGoldTerms)
    {
        var pattern = $@"(?<!\[(?:gold|red|green|blue|purple)\])(?<!\[/)\b({System.Text.RegularExpressions.Regex.Escape(term)})\b(?!\])";
        var regex = new System.Text.RegularExpressions.Regex(pattern);
       
        text = regex.Replace(text, match =>
        {
            if (IsInsideColorTag(text, match.Index))
                return match.Value;
           
            return $"[gold]{match.Value}[/gold]";
        });
    }
    
    foreach (var term in sortedPurpleTerms)
    {
        var pattern = $@"(?<!\[(?:gold|red|green|blue|purple)\])(?<!\[/)\b({System.Text.RegularExpressions.Regex.Escape(term)})\b(?!\])";
        var regex = new System.Text.RegularExpressions.Regex(pattern);
       
        text = regex.Replace(text, match =>
        {
            if (IsInsideColorTag(text, match.Index))
                return match.Value;
           
            return $"[purple]{match.Value}[/purple]";
        });
    }
   
    return text;
}

private bool IsInsideColorTag(string text, int position)
{
   var beforeText = text.Substring(0, position);
  
   var openTags = new System.Text.RegularExpressions.Regex(@"\[(gold|red|green|blue|purple)\]").Matches(beforeText).Count;
   var closeTags = new System.Text.RegularExpressions.Regex(@"\[/(gold|red|green|blue|purple)\]").Matches(beforeText).Count;
  
   return openTags > closeTags;
}

private HashSet<string> GetCustomGoldTerms()
{
    return new HashSet<string>
    {
        "Rest Site",
        "Act",
        "Byrdonis Egg",
        "Enchant",
        "Enchanted",
        "Relic",
        "Relics"
    };
}

private HashSet<string> GetCustomRedTerms()
{
    return new HashSet<string>
    {
        "Curse",
        "Curses"
    };
}
  
private Control CreateFlowContainerWithKeywords(string line, float contentWidth)
{
    var flowContainer = new HFlowContainer();
    flowContainer.AddThemeConstantOverride("h_separation", 0); // Back to zero
    flowContainer.AddThemeConstantOverride("v_separation", 2);
    flowContainer.CustomMinimumSize = new Vector2(contentWidth, 0);

    ProcessColorTags(line, flowContainer);

    return flowContainer;
}
  
   private Control ProcessColorTags(string text, HFlowContainer flowContainer)
   {
       var colorPatterns = new Dictionary<string, Color>
       {
           { "gold", StsColors.gold },
           { "red", StsColors.red },
           { "green", StsColors.green },
           { "blue", StsColors.blue },
           { "purple", StsColors.purple },
       };
      
       string remainingText = text;
       Color? currentColor = null;
      
       while (remainingText.Length > 0)
       {
           string foundColorTag = null;
           int earliestTagIndex = int.MaxValue;
           bool isClosingTag = false;
          
           foreach (var colorName in colorPatterns.Keys)
           {
               int openIndex = remainingText.IndexOf($"[{colorName}]");
               int closeIndex = remainingText.IndexOf($"[/{colorName}]");
              
               if (openIndex >= 0 && openIndex < earliestTagIndex)
               {
                   earliestTagIndex = openIndex;
                   foundColorTag = colorName;
                   isClosingTag = false;
               }
              
               if (closeIndex >= 0 && closeIndex < earliestTagIndex)
               {
                   earliestTagIndex = closeIndex;
                   foundColorTag = colorName;
                   isClosingTag = true;
               }
           }
          
           if (foundColorTag == null)
           {
               ProcessTextSegment(remainingText, flowContainer, currentColor);
               break;
           }
          
           if (earliestTagIndex > 0)
           {
               string beforeText = remainingText.Substring(0, earliestTagIndex);
               ProcessTextSegment(beforeText, flowContainer, currentColor);
           }
          
           if (isClosingTag)
           {
               currentColor = null;
               remainingText = remainingText.Substring(earliestTagIndex + foundColorTag.Length + 3);
           }
           else
           {
               currentColor = colorPatterns[foundColorTag];
               remainingText = remainingText.Substring(earliestTagIndex + foundColorTag.Length + 2);
           }
       }
      
       return flowContainer;
   }
  
private void ProcessTextSegment(string text, HFlowContainer flowContainer, Color? textColor)
{
    var multiWordMatches = new List<(int index, int length, int type, string text)>();

    foreach (var potionName in _potionNameTooltips.Keys)
    {
        if (potionName.Contains(" "))
        {
            int index = text.IndexOf(potionName);
            if (index >= 0)
            {
                multiWordMatches.Add((index, potionName.Length, 4, potionName));
            }
        }
    }

    foreach (var relicName in _relicNameTooltips.Keys)
    {
        if (relicName.Contains(" "))
        {
            int index = text.IndexOf(relicName);
            if (index >= 0)
            {
                multiWordMatches.Add((index, relicName.Length, 2, relicName));
            }
        }
    }

    multiWordMatches = multiWordMatches.OrderBy(m => m.index).ToList();

    if (multiWordMatches.Count > 0)
    {
        int currentPos = 0;

        foreach (var match in multiWordMatches)
        {
            if (match.index > currentPos)
            {
                string beforeText = text.Substring(currentPos, match.index - currentPos);
                ProcessTextSegmentSingleWords(beforeText, flowContainer, textColor);
            }

            // Split the multi-word keyword into individual words with controlled spacing
            AddMultiWordKeyword(flowContainer, match.text, match.type, textColor);

            currentPos = match.index + match.length;
        }

        if (currentPos < text.Length)
        {
            string remainingText = text.Substring(currentPos);
            ProcessTextSegmentSingleWords(remainingText, flowContainer, textColor);
        }

        return;
    }

    ProcessTextSegmentSingleWords(text, flowContainer, textColor);
}

private void AddMultiWordKeyword(HFlowContainer flowContainer, string fullText, int matchType, Color? textColor)
{
    string[] words = fullText.Split(' ');

    for (int i = 0; i < words.Length; i++)
    {
        Label label;
        switch (matchType)
        {
            case 2:
                label = CreateRelicNameLabel(words[i], textColor);
                break;
            case 4:
                label = CreatePotionNameLabel(words[i], textColor);
                break;
            default:
                label = CreateColoredLabel(words[i], textColor);
                break;
        }

        label.SetMeta("full_keyword", fullText);
        label.SetMeta("keyword_type", matchType);
        
        flowContainer.AddChild(label);

        if (i < words.Length - 1)
        {
            var spaceLabel = CreateSpaceLabel(textColor);
            spaceLabel.SetMeta("full_keyword", fullText);
            spaceLabel.SetMeta("keyword_type", matchType);
            spaceLabel.MouseFilter = Control.MouseFilterEnum.Stop;
            flowContainer.AddChild(spaceLabel);
        }
    }
}

private Label CreateSpaceLabel(Color? textColor)
{
    var label = new Label();
    label.Text = "";
    label.CustomMinimumSize = new Vector2(5, 0);
    return label;
}

private void ProcessTextSegmentSingleWords(string text, HFlowContainer flowContainer, Color? textColor, bool trailingSpace = false)
{
    // Split but preserve spacing information
    var tokens = TokenizePreservingSpaces(text);

    foreach (var token in tokens)
    {
        if (token.isSpace)
        {
            flowContainer.AddChild(CreateSpaceLabel(textColor));
            continue;
        }

        string word = token.text;

        // Find the best (longest) match
        string bestMatch = null;
        int bestMatchIndex = -1;
        int bestMatchType = -1;

        CheckForMatch(word, _keywordTooltips.Keys, 0, ref bestMatch, ref bestMatchIndex, ref bestMatchType);
        CheckForMatch(word, _cardNameTooltips.Keys, 1, ref bestMatch, ref bestMatchIndex, ref bestMatchType);
        CheckForMatch(word, _relicNameTooltips.Keys.Where(r => !r.Contains(" ")), 2, ref bestMatch, ref bestMatchIndex, ref bestMatchType);
        CheckForMatch(word, _enchantmentNameTooltips.Keys, 3, ref bestMatch, ref bestMatchIndex, ref bestMatchType);
        CheckForMatch(word, _potionNameTooltips.Keys.Where(p => !p.Contains(" ")), 4, ref bestMatch, ref bestMatchIndex, ref bestMatchType);

        if (bestMatch != null)
        {
            // Text before the match
            if (bestMatchIndex > 0)
            {
                flowContainer.AddChild(CreateColoredLabel(word.Substring(0, bestMatchIndex), textColor));
            }

            // The matched keyword/card/relic/etc
            switch (bestMatchType)
            {
                case 0:
                    flowContainer.AddChild(CreateKeywordLabel(bestMatch));
                    break;
                case 1:
                    flowContainer.AddChild(CreateCardNameLabel(bestMatch, textColor));
                    break;
                case 2:
                    flowContainer.AddChild(CreateRelicNameLabel(bestMatch, textColor));
                    break;
                case 3:
                    flowContainer.AddChild(CreateEnchantmentNameLabel(bestMatch, textColor));
                    break;
                case 4:
                    flowContainer.AddChild(CreatePotionNameLabel(bestMatch, textColor));
                    break;
            }

            // Text after the match (punctuation, trailing characters) - no space before
            int afterMatchIndex = bestMatchIndex + bestMatch.Length;
            if (afterMatchIndex < word.Length)
            {
                flowContainer.AddChild(CreateColoredLabel(word.Substring(afterMatchIndex), textColor));
            }
        }
        else
        {
            flowContainer.AddChild(CreateColoredLabel(word, textColor));
        }
    }
}

private void CheckForMatch(string word, IEnumerable<string> candidates, int matchType, 
    ref string bestMatch, ref int bestMatchIndex, ref int bestMatchType)
{
    foreach (var candidate in candidates)
    {
        int index = word.IndexOf(candidate);
        if (index >= 0 && (bestMatch == null || candidate.Length > bestMatch.Length))
        {
            int endIndex = index + candidate.Length;
            if (endIndex == word.Length || !char.IsLetter(word[endIndex]))
            {
                bestMatch = candidate;
                bestMatchIndex = index;
                bestMatchType = matchType;
            }
        }
    }
}

private List<(string text, bool isSpace)> TokenizePreservingSpaces(string text)
{
    var tokens = new List<(string text, bool isSpace)>();
    
    int i = 0;
    while (i < text.Length)
    {
        if (text[i] == ' ')
        {
            tokens.Add((" ", true));
            i++;
        }
        else
        {
            int start = i;
            while (i < text.Length && text[i] != ' ')
            {
                i++;
            }
            tokens.Add((text.Substring(start, i - start), false));
        }
    }
    
    return tokens;
}

private bool IsPunctuationOnly(string text)
{
    foreach (char c in text)
    {
        if (char.IsLetterOrDigit(c))
            return false;
    }
    return true;
}

private Label CreatePunctuationLabel(string text, Color? textColor)
{
    var label = _labelFactory.CreateColoredLabel(text, textColor);
    // Negative margin to pull punctuation closer to the previous element
    label.AddThemeConstantOverride("margin_left", -4);
    return label;
}
   public Label CreateKeywordLabel(string keyword)
   {
       return _labelFactory.CreateKeywordLabel(keyword);
   }
 
   public Label CreateCardNameLabel(string cardName, Color? textColor)
   {
       return _labelFactory.CreateCardNameLabel(cardName, textColor);
   }
  
   public Label CreateRelicNameLabel(string relicName, Color? textColor)
   {
       return _labelFactory.CreateRelicNameLabel(relicName, textColor);
   }
   
   public Label CreateEnchantmentNameLabel(string enchantmentName, Color? textColor)
   {
       return _labelFactory.CreateEnchantmentNameLabel(enchantmentName, textColor);
   }
   
   public Label CreatePotionNameLabel(string potionName, Color? textColor)
   {
       return _labelFactory.CreatePotionNameLabel(potionName, textColor);
   }

   public bool TryGetPotionNameTooltip(string potionName, out IEnumerable<IHoverTip> tooltips)
   {
       return _potionNameTooltips.TryGetValue(potionName, out tooltips);
   }
 
   public Label CreateColoredLabel(string text, Color? color)
   {
       return _labelFactory.CreateColoredLabel(text, color);
   }
 
   public bool TryGetKeywordTooltip(string keyword, out IHoverTip tooltip)
   {
       return _keywordTooltips.TryGetValue(keyword, out tooltip);
   }
 
   public bool TryGetCardNameTooltip(string cardName, out CardModel card)
   {
       return _cardNameTooltips.TryGetValue(cardName, out card);
   }
  
   public bool TryGetRelicNameTooltip(string relicName, out IEnumerable<IHoverTip> tooltips)
   {
       return _relicNameTooltips.TryGetValue(relicName, out tooltips);
   }
   
   public bool TryGetEnchantmentNameTooltip(string enchantmentName, out IEnumerable<IHoverTip> tooltips)
   {
       if (_enchantmentNameTooltips.TryGetValue(enchantmentName, out var factory))
       {
           tooltips = factory();
           return true;
       }
       tooltips = null;
       return false;
   }
}