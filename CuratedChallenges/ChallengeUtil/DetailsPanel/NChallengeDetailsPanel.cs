using CuratedChallenges.ChallengeUtil;
using CuratedChallenges.ChallengeUtil.DetailsPanel;
using Godot;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Relics;
using MegaCrit.Sts2.Core.Nodes.Screens.RunHistoryScreen;
using MegaCrit.Sts2.Core.RichTextTags;

namespace CuratedChallenges.Panels;

public class NChallengeDetailsPanel : Control
{
  private const string LOG_TAG = "[ChallengeDetailsPanel]";


  private VBoxContainer _container;
  private Label _nameLabel;
  private Label _deckHeader;
  private Control _deckContent;
  private Label _relicsHeader;
  private HBoxContainer _relicsContainer;
  private Label _rulesHeader;
  private Control _rulesContent;
  private Label _winConditionsHeader;
  private Control _winConditionsContent;
  private List<CardModel> _allCards = new List<CardModel>();
  private List<RelicModel> _allRelics = new List<RelicModel>();
  private Control _currentTooltipAnchor = null;
  private TextureRect _neowBonusIcon;
  
  private ChallengeDetailsPanelLabelFactory _labelFactory;
  private ChallengeDetailsPanelTextProcessor _textProcessor;


  private static readonly float SECTION_SPACING = 20f;


  public NChallengeDetailsPanel()
  {
      Initialize();
  }


  private void Initialize()
  {
      _labelFactory = new ChallengeDetailsPanelLabelFactory();
      _textProcessor = new ChallengeDetailsPanelTextProcessor(_labelFactory);
    
      // Main container
      _container = new VBoxContainer();
      _container.AddThemeConstantOverride("separation", (int)SECTION_SPACING);
      AddChild(_container);
    
      // Challenge name
      _nameLabel = _labelFactory.CreateHeaderLabel("");
      // _container.AddChild(_nameLabel);
    
      // Starting Deck section
      _deckHeader = _labelFactory.CreateHeaderLabel("Starting Deck");
      _container.AddChild(_deckHeader);
    
      _deckContent = new VBoxContainer();
      ((VBoxContainer)_deckContent).AddThemeConstantOverride("separation", 3);
      _container.AddChild(_deckContent);
    
      // Starting Relics section
      _relicsHeader = _labelFactory.CreateHeaderLabel("Starting Relics");
      _container.AddChild(_relicsHeader);
    
      _relicsContainer = new HBoxContainer();
      _relicsContainer.AddThemeConstantOverride("separation", 10);
      _container.AddChild(_relicsContainer);
      
// Special Rules section (with optional Neow icon)
      var rulesHeaderContainer = new HBoxContainer();
      rulesHeaderContainer.AddThemeConstantOverride("separation", 10);
      _container.AddChild(rulesHeaderContainer);

      _rulesHeader = _labelFactory.CreateHeaderLabel("Special Rules");
      rulesHeaderContainer.AddChild(_rulesHeader);

      _neowBonusIcon = new TextureRect();
      _neowBonusIcon.Texture = GD.Load<Texture2D>("res://images/ui/run_history/neow.png");
      _neowBonusIcon.ExpandMode = TextureRect.ExpandModeEnum.FitHeightProportional;
      _neowBonusIcon.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
      _neowBonusIcon.CustomMinimumSize = new Vector2(40, 40);
      _neowBonusIcon.Visible = false;
      rulesHeaderContainer.AddChild(_neowBonusIcon);
    
      _rulesContent = new VBoxContainer();
      _container.AddChild(_rulesContent);
    
      // Win Conditions section
      _winConditionsHeader = _labelFactory.CreateHeaderLabel("Win Conditions");
      _container.AddChild(_winConditionsHeader);
    
      _winConditionsContent = new VBoxContainer();
      _container.AddChild(_winConditionsContent);
    
      Visible = false;
  }


  public void PositionPanel()
  {
      var viewportSize = GetViewportRect().Size;
    
      float panelX = viewportSize.X * 0.6f;
      float panelY = viewportSize.Y * 0.3f;
      float panelWidth = viewportSize.X * 0.35f;
    
      Position = new Vector2(panelX, panelY);
      CustomMinimumSize = new Vector2(panelWidth, 0);
    
      float contentWidth = panelWidth - 40;
      _deckContent.CustomMinimumSize = new Vector2(contentWidth, 0);
      _rulesContent.CustomMinimumSize = new Vector2(contentWidth, 0);
      _winConditionsContent.CustomMinimumSize = new Vector2(contentWidth, 0);
  }


  public void DisplayChallenge(Challenge challenge)
  {
      _neowBonusIcon.Visible = challenge.Definition?.AllowNeowBonuses ?? false;
      // Clear previous deck entries
      foreach (Node child in _deckContent.GetChildren())
      {
          child.QueueFree();
      }
    
      _allCards.Clear();
    
      // Display starting deck using NDeckHistoryEntry
      var cardGroups = challenge.StartingDeck.GroupBy(card => card.Id);
      foreach (var group in cardGroups)
      {
          CardModel card = group.First();
          int count = group.Count();
        
          _allCards.Add(card);
        
          NDeckHistoryEntry entry = NDeckHistoryEntry.Create(card, count, Enumerable.Empty<int>());
          entry.Clicked += OnCardClicked;
          _deckContent.AddChild(entry);
      }
    
      // Display starting relics using NRelicBasicHolder
      ClearRelics();
      _allRelics.Clear();
    
      foreach (var relic in challenge.StartingRelics)
      {
          _allRelics.Add(relic);
        
          NRelicBasicHolder holder = NRelicBasicHolder.Create(relic);
          holder.MouseDefaultCursorShape = Control.CursorShape.Help;
        
          holder.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(_ => OnRelicClicked(holder.Relic)));
        
          _relicsContainer.AddChild(holder);
      }
    
      // Display special rules with keyword tooltips
      string[] rulesLines = challenge.SpecialRules.Split(new[] { " \n " }, StringSplitOptions.RemoveEmptyEntries);
      string formattedRules = string.Join("\n", rulesLines.Select(line => "- " + line.Trim()));
     
      // Clear old rules content
      foreach (Node child in _rulesContent.GetChildren())
      {
          child.QueueFree();
      }
     
      float contentWidth = _rulesContent.CustomMinimumSize.X > 0 ? _rulesContent.CustomMinimumSize.X : 400f;
      var rulesContainer = _textProcessor.CreateRichTextLabelWithKeywords(formattedRules, contentWidth);
      AttachTooltipHandlers(rulesContainer);
      _rulesContent.AddChild(rulesContainer);
    
      // Display win conditions with keyword tooltips
      string[] winConditionLines = challenge.WinConditions.Split(new[] { " \n " }, StringSplitOptions.RemoveEmptyEntries);
      string formattedWinConditions = string.Join("\n", winConditionLines.Select(line => "- " + line.Trim()));
     
      // Clear old win conditions content
      foreach (Node child in _winConditionsContent.GetChildren())
      {
          child.QueueFree();
      }
     
      var winConditionsContainer = _textProcessor.CreateRichTextLabelWithKeywords(formattedWinConditions, contentWidth);
      AttachTooltipHandlers(winConditionsContainer);
      _winConditionsContent.AddChild(winConditionsContainer);
    
      Visible = true;
    
      // Force layout recalculation
      _container.QueueSort();
      CallDeferred("queue_sort");
    
      // Log after making visible
      CallDeferred("_LogPanelState");
  }


private void AttachTooltipHandlers(Control container)
{
    foreach (Node child in container.GetChildren())
    {
        if (child is Label label && label.MouseFilter == MouseFilterEnum.Stop)
        {
            // Check for multi-word keyword metadata first
            if (label.HasMeta("full_keyword"))
            {
                string fullKeyword = label.GetMeta("full_keyword").AsString();
                int keywordType = label.GetMeta("keyword_type").AsInt32();

                if (keywordType == 2 && _textProcessor.TryGetRelicNameTooltip(fullKeyword, out _))
                {
                    label.Connect("mouse_entered", Callable.From(() => OnRelicNameLabelEntered(label, fullKeyword)));
                    label.Connect("mouse_exited", Callable.From(() => OnRelicNameLabelExited(label)));
                }
                else if (keywordType == 4 && _textProcessor.TryGetPotionNameTooltip(fullKeyword, out _))
                {
                    label.Connect("mouse_entered", Callable.From(() => OnPotionNameLabelEntered(label, fullKeyword)));
                    label.Connect("mouse_exited", Callable.From(() => OnPotionNameLabelExited(label)));
                }
            }
            // Existing single-word checks...
            else if (_textProcessor.TryGetKeywordTooltip(label.Text, out IHoverTip keywordTip))
            {
                label.Connect("mouse_entered", Callable.From(() => OnKeywordLabelEntered(label, label.Text)));
                label.Connect("mouse_exited", Callable.From(() => OnKeywordLabelExited(label)));
            }
            else if (_textProcessor.TryGetCardNameTooltip(label.Text, out CardModel card))
            {
                label.Connect("mouse_entered", Callable.From(() => OnCardNameLabelEntered(label, label.Text)));
                label.Connect("mouse_exited", Callable.From(() => OnCardNameLabelExited(label)));
            }
            else if (_textProcessor.TryGetRelicNameTooltip(label.Text, out IEnumerable<IHoverTip> relicTips))
            {
                label.Connect("mouse_entered", Callable.From(() => OnRelicNameLabelEntered(label, label.Text)));
                label.Connect("mouse_exited", Callable.From(() => OnRelicNameLabelExited(label)));
            }
            else if (_textProcessor.TryGetEnchantmentNameTooltip(label.Text, out IEnumerable<IHoverTip> enchantmentTips))
            {
                label.Connect("mouse_entered", Callable.From(() => OnEnchantmentNameLabelEntered(label, label.Text)));
                label.Connect("mouse_exited", Callable.From(() => OnEnchantmentNameLabelExited(label)));
            }
            else if (_textProcessor.TryGetPotionNameTooltip(label.Text, out IEnumerable<IHoverTip> potionTips))
            {
                label.Connect("mouse_entered", Callable.From(() => OnPotionNameLabelEntered(label, label.Text)));
                label.Connect("mouse_exited", Callable.From(() => OnPotionNameLabelExited(label)));
            }
        }

        if (child is Control childControl)
        {
            AttachTooltipHandlers(childControl);
        }
    }
}

private void OnEnchantmentNameLabelEntered(Label label, string enchantmentName)
{
    
    Node parent = label.GetParent();
    while (parent != null)
    {
        if (parent == _rulesContent || parent == _winConditionsContent)
        {
            _currentTooltipAnchor = (Control)parent;
            break;
        }
        parent = parent.GetParent();
    }
    
    if (_currentTooltipAnchor == null)
    {
        _currentTooltipAnchor = _rulesContent;
    }
    
    if (_textProcessor.TryGetEnchantmentNameTooltip(enchantmentName, out IEnumerable<IHoverTip> enchantmentTips))
    {
        var tips = enchantmentTips.ToList();
        var hoverTipSet = NHoverTipSet.CreateAndShow(_currentTooltipAnchor, tips);
        
        var alignment = HoverTip.GetHoverTipAlignment(_currentTooltipAnchor);
        hoverTipSet?.SetAlignment(_currentTooltipAnchor, alignment);
    }
}

private void OnEnchantmentNameLabelExited(Label label)
{
    if (_currentTooltipAnchor != null)
    {
        NHoverTipSet.Remove(_currentTooltipAnchor);
    }
}

private void OnPotionNameLabelEntered(Label label, string potionName)
{
    
    Node parent = label.GetParent();
    while (parent != null)
    {
        if (parent == _rulesContent || parent == _winConditionsContent)
        {
            _currentTooltipAnchor = (Control)parent;
            break;
        }
        parent = parent.GetParent();
    }
    
    if (_currentTooltipAnchor == null)
    {
        _currentTooltipAnchor = _rulesContent;
    }
    
    if (_textProcessor.TryGetPotionNameTooltip(potionName, out IEnumerable<IHoverTip> potionTips))
    {
        var tips = potionTips.ToList();
        var hoverTipSet = NHoverTipSet.CreateAndShow(_currentTooltipAnchor, tips);
        
        var alignment = HoverTip.GetHoverTipAlignment(_currentTooltipAnchor);
        hoverTipSet?.SetAlignment(_currentTooltipAnchor, alignment);
    }
}

private void OnPotionNameLabelExited(Label label)
{
    if (_currentTooltipAnchor != null)
    {
        NHoverTipSet.Remove(_currentTooltipAnchor);
    }
}


  private void OnKeywordLabelEntered(Label label, string keyword)
  {
  
      // Find which content container this label belongs to
      Node parent = label.GetParent();
      while (parent != null)
      {
          if (parent == _rulesContent || parent == _winConditionsContent)
          {
              _currentTooltipAnchor = (Control)parent;
              break;
          }
          parent = parent.GetParent();
      }
  
      if (_currentTooltipAnchor == null)
      {
          _currentTooltipAnchor = _rulesContent;
      }
  
      if (_textProcessor.TryGetKeywordTooltip(keyword, out IHoverTip hoverTip))
      {
          var tips = new List<IHoverTip> { hoverTip };
          var hoverTipSet = NHoverTipSet.CreateAndShow(_currentTooltipAnchor, tips);
      
          var alignment = HoverTip.GetHoverTipAlignment(_currentTooltipAnchor);
          hoverTipSet?.SetAlignment(_currentTooltipAnchor, alignment);
      }
  }


  private void OnKeywordLabelExited(Label label)
  {
      if (_currentTooltipAnchor != null)
      {
          NHoverTipSet.Remove(_currentTooltipAnchor);
      }
  }


  private void OnCardNameLabelEntered(Label label, string cardName)
  {
      Node parent = label.GetParent();
      while (parent != null)
      {
          if (parent == _rulesContent || parent == _winConditionsContent)
          {
              _currentTooltipAnchor = (Control)parent;
              break;
          }
          parent = parent.GetParent();
      }
  
      if (_currentTooltipAnchor == null)
      {
          _currentTooltipAnchor = _rulesContent;
      }
  
      if (_textProcessor.TryGetCardNameTooltip(cardName, out CardModel card))
      {
          var hoverTip = HoverTipFactory.FromCard(card);
          var tips = new List<IHoverTip> { hoverTip };
          var hoverTipSet = NHoverTipSet.CreateAndShow(_currentTooltipAnchor, tips);
      
          var alignment = HoverTip.GetHoverTipAlignment(_currentTooltipAnchor);
          hoverTipSet?.SetAlignment(_currentTooltipAnchor, alignment);
      }
  }


  private void OnCardNameLabelExited(Label label)
  {
      if (_currentTooltipAnchor != null)
      {
          NHoverTipSet.Remove(_currentTooltipAnchor);
      }
  }
  
  private void OnRelicNameLabelEntered(Label label, string relicName)
  {
      Node parent = label.GetParent();
      while (parent != null)
      {
          if (parent == _rulesContent || parent == _winConditionsContent)
          {
              _currentTooltipAnchor = (Control)parent;
              break;
          }
          parent = parent.GetParent();
      }

      if (_currentTooltipAnchor == null)
      {
          _currentTooltipAnchor = _rulesContent;
      }

      if (_textProcessor.TryGetRelicNameTooltip(relicName, out IEnumerable<IHoverTip> relicTips))
      {
          var tips = relicTips.ToList();
          var hoverTipSet = NHoverTipSet.CreateAndShow(_currentTooltipAnchor, tips);
    
          var alignment = HoverTip.GetHoverTipAlignment(_currentTooltipAnchor);
          hoverTipSet?.SetAlignment(_currentTooltipAnchor, alignment);
      }
  }

  private void OnRelicNameLabelExited(Label label)
  {
      if (_currentTooltipAnchor != null)
      {
          NHoverTipSet.Remove(_currentTooltipAnchor);
      }
  }


  private void OnCardClicked(NDeckHistoryEntry entry)
  {
      NGame.Instance.GetInspectCardScreen().Open(_allCards, _allCards.IndexOf(entry.Card));
  }


  private void OnRelicClicked(NRelic relic)
  {
      NGame.Instance.GetInspectRelicScreen().Open(_allRelics, relic.Model);
  }

  private void ClearRelics()
  {
      foreach (Node child in _relicsContainer.GetChildren())
      {
          child.QueueFree();
      }
  }


  public new void Hide()
  {
      Visible = false;
  }
}