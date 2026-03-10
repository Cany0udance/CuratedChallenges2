using System.Reflection;
using CuratedChallenges.ChallengeUtil;
using CuratedChallenges.Panels;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.Entities.UI;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer.Game.Lobby;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Audio;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Unlocks;

namespace CuratedChallenges.Screens;

public class NChallengesScreen : NSubmenu, ICharacterSelectButtonDelegate
{
    private const string CHAR_SELECT_BUTTON_SCENE = "res://scenes/screens/char_select/char_select_button.tscn";
    
    private Control _charButtonContainer;
    private VBoxContainer _challengeListContainer;
    private NCharacterSelectButton _selectedButton;
    private StartRunLobby _lobby;
    private CharacterModel _selectedCharacter;
    private List<Challenge> _currentChallenges;
    private Challenge _selectedChallenge;
    private NChallengeDetailsPanel _detailsPanel;
    private NConfirmButton _embarkButton;
    private ScrollContainer _scrollContainer;
    private Control _scrollableContent;
    private NChallengeTypeToggle _challengeTypeToggle;
    private NAscensionPanel _ascensionPanel;
    
    public StartRunLobby Lobby => _lobby;
    
    private const float BUTTON_CONTAINER_Y = 50f;
    private const float BUTTON_SPACING = 20f;
    private const float CHALLENGE_LIST_X = 450f;
    private const float CHALLENGE_SPACING = 20f;
    private const float TOGGLE_Y = 200f;
    private const float ASCENSION_PANEL_Y = 250f;
    
    protected override Control InitialFocusedControl
    {
        get
        {
            if (_charButtonContainer?.GetChildCount() > 0)
            {
                return _charButtonContainer.GetChild<Control>(0);
            }
            return null;
        }
    }
    
public override void _Ready()
{
    try
    {
        SetAnchorsPreset(LayoutPreset.FullRect);
        
        float screenHeight = GetViewportRect().Size.Y;
        float screenWidth = GetViewportRect().Size.X;
        
        _lobby = new StartRunLobby(GameMode.Standard, new NetSingleplayerGameService(), null, 1);
        _currentChallenges = new List<Challenge>();
        
        var background = new ColorRect();
        background.Color = new Color(0.1f, 0.1f, 0.15f, 1.0f);
        background.SetAnchorsPreset(LayoutPreset.FullRect);
        AddChild(background);
        
        _scrollContainer = new ScrollContainer();
        _scrollContainer.Name = "ScrollContainer";
        _scrollContainer.Position = Vector2.Zero;
        _scrollContainer.Size = new Vector2(screenWidth, screenHeight);
        _scrollContainer.MouseFilter = MouseFilterEnum.Pass;
        AddChild(_scrollContainer);
        
        _scrollableContent = new Control();
        _scrollableContent.Name = "ScrollableContent";
        _scrollableContent.CustomMinimumSize = new Vector2(screenWidth, screenHeight * 2f);
        _scrollContainer.AddChild(_scrollableContent);
        
        _charButtonContainer = new HBoxContainer();
        _charButtonContainer.Name = "CharButtonContainer";
        _charButtonContainer.Position = new Vector2(0, BUTTON_CONTAINER_Y);
        _charButtonContainer.AddThemeConstantOverride("separation", (int)BUTTON_SPACING);
        ((HBoxContainer)_charButtonContainer).Alignment = BoxContainer.AlignmentMode.Center;
        _scrollableContent.AddChild(_charButtonContainer);
        
        _challengeTypeToggle = new NChallengeTypeToggle();
        _challengeTypeToggle.Name = "ChallengeTypeToggle";
        _challengeTypeToggle.Position = new Vector2(0, TOGGLE_Y);
        _challengeTypeToggle.TypeChanged += OnChallengeTypeChanged;
        _scrollableContent.AddChild(_challengeTypeToggle);
        
        var ascensionScene = ResourceLoader.Load<PackedScene>("res://scenes/screens/ascension_panel.tscn");
        _ascensionPanel = ascensionScene.Instantiate<NAscensionPanel>();
        _ascensionPanel.Name = "AscensionPanel";
        _ascensionPanel.Scale = new Vector2(0.5f, 0.5f);
        _ascensionPanel.Position = new Vector2(0, ASCENSION_PANEL_Y);
        _ascensionPanel.Visible = false;
        _scrollableContent.AddChild(_ascensionPanel);
        _ascensionPanel.Initialize(MultiplayerUiMode.Singleplayer);
        
        _challengeListContainer = new VBoxContainer();
        _challengeListContainer.Name = "ChallengeListContainer";
        _challengeListContainer.Position = new Vector2(CHALLENGE_LIST_X, screenHeight * 0.3f);
        _challengeListContainer.AddThemeConstantOverride("separation", (int)CHALLENGE_SPACING);
        _scrollableContent.AddChild(_challengeListContainer);
        
        _detailsPanel = new NChallengeDetailsPanel();
        _scrollableContent.AddChild(_detailsPanel);
        _detailsPanel.PositionPanel();
        
        var backButtonScene = ResourceLoader.Load<PackedScene>("res://scenes/ui/back_button.tscn");
        var backButton = backButtonScene.Instantiate<NBackButton>();
        backButton.Name = "BackButton";
        AddChild(backButton);
        
        var embarkButtonScene = ResourceLoader.Load<PackedScene>("res://scenes/ui/confirm_button.tscn");
        _embarkButton = embarkButtonScene.Instantiate<NConfirmButton>();
        _embarkButton.Name = "EmbarkButton";
        _embarkButton.Visible = false;
        _embarkButton.Disable();
        AddChild(_embarkButton);
        
        InitCharacterButtons();
        ConnectSignals();
        
        // Enable the back button that NSubmenu.ConnectSignals() captured
        var backButtonField = typeof(NSubmenu).GetField("_backButton",
            BindingFlags.NonPublic | BindingFlags.Instance);
        var backButtonFromBase = (NBackButton)backButtonField?.GetValue(this);
        backButtonFromBase?.Enable();
    }
    catch (Exception e)
    {
        Log.Error($"[ChallengesScreen] Error in _Ready: {e}");
    }
}
    
    private void OnChallengeTypeChanged(NChallengeTypeToggle.ChallengeType type)
    {
        if (_selectedCharacter != null)
        {
            LoadChallengesForCharacter(_selectedCharacter);
        }
    }
    
    protected override void ConnectSignals()
    {
        base.ConnectSignals();
        
        _embarkButton?.Connect(NClickableControl.SignalName.Released, 
            Callable.From<NButton>(OnEmbarkPressed));
    }
    
    private void InitCharacterButtons()
    {
        var buttonScene = ResourceLoader.Load<PackedScene>(CHAR_SELECT_BUTTON_SCENE);
        
        foreach (CharacterModel character in ModelDb.AllCharacters)
        {
            var button = buttonScene.Instantiate<NCharacterSelectButton>();
            button.Name = character.Id.Entry + "_button";
            _charButtonContainer.AddChild(button);
            button.Init(character, this);
        }
    }
    
    private void CenterButtonContainer()
    {
        float totalWidth = 0f;
        int childCount = _charButtonContainer.GetChildCount();
        
        for (int i = 0; i < childCount; i++)
        {
            var child = _charButtonContainer.GetChild<Control>(i);
            totalWidth += child.Size.X;
        }
        
        totalWidth += BUTTON_SPACING * (childCount - 1);
        
        float screenWidth = GetViewportRect().Size.X;
        float xPos = (screenWidth - totalWidth) / 2f;
        
        _charButtonContainer.Position = new Vector2(xPos, BUTTON_CONTAINER_Y);
    }
    
    private void LoadChallengesForCharacter(CharacterModel character)
    {
        foreach (Node child in _challengeListContainer.GetChildren())
        {
            child.QueueFree();
        }
        _currentChallenges.Clear();
        _selectedChallenge = null;
        
        _embarkButton.Visible = false;
        _embarkButton.Disable();
        _detailsPanel.Hide();
        _ascensionPanel.Visible = false;
        
        var challengeDefs = _challengeTypeToggle.CurrentType == NChallengeTypeToggle.ChallengeType.Individual
            ? ChallengeRegistry.GetChallengesForCharacter(character.Id).Where(c => !c.IsShared)
            : ChallengeRegistry.GetChallengesForCharacter(character.Id).Where(c => c.IsShared);
        
        foreach (var challengeDef in challengeDefs)
        {
            var challenge = new Challenge(challengeDef, character);
            _currentChallenges.Add(challenge);
            
            var challengeItem = new NChallengeItem();
            challengeItem.Init(challenge);
            challengeItem.Selected += OnChallengeSelected;
            _challengeListContainer.AddChild(challengeItem);
        }
    }
    
    private void CenterToggle()
    {
        float screenWidth = GetViewportRect().Size.X;
        float toggleWidth = 250f;
        float xPos = (screenWidth - toggleWidth) / 2f;
        
        _challengeTypeToggle.Position = new Vector2(xPos, TOGGLE_Y);
    }
    
    private void OnChallengeSelected(NChallengeItem selectedItem)
    {
        if (!selectedItem.IsSelected)
        {
            _selectedChallenge = null;
            _detailsPanel.Hide();
            _ascensionPanel.Visible = false;
            _embarkButton.Visible = false;
            _embarkButton.Disable();
            return;
        }
        
        foreach (NChallengeItem item in _challengeListContainer.GetChildren().OfType<NChallengeItem>())
        {
            if (item != selectedItem)
            {
                item.Deselect();
            }
        }
        
        _selectedChallenge = selectedItem.Challenge;
        _detailsPanel.DisplayChallenge(_selectedChallenge);
        _ascensionPanel.Visible = true;
        _embarkButton.Visible = true;
        _embarkButton.Enable();
    }
    
    private void OnEmbarkPressed(NButton _)
    {
        if (_selectedChallenge == null || _selectedCharacter == null)
        {
            return;
        }
        
        _embarkButton.Disable();
        TaskHelper.RunSafely(StartChallengeRun());
    }
    
    private async Task StartChallengeRun()
    {
        ChallengeModifier.IsStartingChallengeRun = true;
        NAudioManager.Instance?.StopMusic();
        
        SfxCmd.Play(_selectedCharacter.CharacterTransitionSfx);
        await NGame.Instance.Transition.FadeOut(transitionPath: _selectedCharacter.CharacterSelectTransitionPath);
        
        var challengeModifier = _selectedChallenge.CreateModifier();
        var modifiers = new List<ModifierModel> { challengeModifier };
        
        string seed = _lobby.Seed ?? SeedHelper.GetRandomSeed();
        
        List<ActModel> acts = ActModel.GetDefaultList().ToList();
        
        // 50/50 chance to replace Overgrowth with Underdocks
        var rng = new Rng((uint)StringHelper.GetDeterministicHashCode(seed));
        if (rng.NextBool())
        {
            acts[0] = ModelDb.Act<Underdocks>();
        }
        
        await NGame.Instance.StartNewSingleplayerRun(
            _selectedCharacter,
            true,
            acts,
            modifiers,
            seed,
            _ascensionPanel.Ascension,
            null
        );
        
        CleanUpLobby();
    }
    
    private void CleanUpLobby()
    {
        _lobby?.CleanUp(true);
        _lobby = null;
    }
    
    public void SelectCharacter(NCharacterSelectButton charSelectButton, CharacterModel characterModel)
    {
        _selectedButton = charSelectButton;
        _selectedCharacter = characterModel;
        
        foreach (NCharacterSelectButton button in _charButtonContainer.GetChildren().OfType<NCharacterSelectButton>())
        {
            if (button != _selectedButton)
            {
                button.Deselect();
            }
        }
        
        LoadChallengesForCharacter(characterModel);
        _ascensionPanel.SetMaxAscension(10);
        _ascensionPanel.SetAscensionLevel(0);
        _ascensionPanel.Visible = false;
    }
    
    public override void OnSubmenuOpened()
    {
        base.OnSubmenuOpened();
        
        foreach (NCharacterSelectButton button in _charButtonContainer.GetChildren().OfType<NCharacterSelectButton>())
        {
            if (!button.IsLocked)
            {
                button.Enable();
            }
            else
            {
                button.UnlockIfPossible();
            }
        }
        
        var firstUnlocked = _charButtonContainer.GetChildren()
            .OfType<NCharacterSelectButton>()
            .FirstOrDefault(b => !b.IsLocked);
        
        firstUnlocked?.Select();
        
        CenterButtonContainer();
        CenterToggle();
        CenterAscensionPanel();
        
        _scrollContainer.ScrollVertical = 0;
    }
    
    private void CenterAscensionPanel()
    {
        float screenWidth = GetViewportRect().Size.X;
        float panelWidth = _ascensionPanel.Size.X * _ascensionPanel.Scale.X;
        float xPos = (screenWidth - panelWidth) / 2f;
        
        _ascensionPanel.Position = new Vector2(xPos, ASCENSION_PANEL_Y);
    }
    
    public override void OnSubmenuClosed()
    {
        base.OnSubmenuClosed();
        
        _ascensionPanel?.Cleanup();
        
        foreach (NCharacterSelectButton button in _charButtonContainer.GetChildren().OfType<NCharacterSelectButton>())
        {
            button.Disable();
        }
        
        _embarkButton?.Disable();
        CleanUpLobby();
    }
}