using CuratedChallenges.ChallengeUtil.Progression;
using Godot;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;

namespace CuratedChallenges.ChallengeUtil;

public class NChallengeItem : Control
{
    private Challenge _challenge;
    private NTickbox _tickbox;
    private Label _nameLabel;
    private bool _isSelected;
    private bool _suppressEvents;
    private Control _completionDot;
    private Control _ascensionIndicator;
    
    public Challenge Challenge => _challenge;
    
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            _isSelected = value;
            if (_tickbox != null && _tickbox.IsNodeReady())
            {
                _suppressEvents = true;
                _tickbox.IsTicked = value;
                _suppressEvents = false;
            }
        }
    }
    
    public event Action<NChallengeItem> Selected;
    
public void Init(Challenge challenge)
{
    if (_challenge != null)
    {
        return;
    }
    _challenge = challenge;
    
    var hbox = new HBoxContainer();
    hbox.AddThemeConstantOverride("separation", 10);
    AddChild(hbox);
    
    bool isCompleted = ChallengeProgressHelper.IsChallengeCompleted(
        challenge.Definition.Id,
        challenge.IsShared,
        challenge.Character.Id
    );
    int highestAscension = ChallengeProgressHelper.GetHighestAscension(
        challenge.Definition.Id,
        challenge.IsShared,
        challenge.Character.Id
    );
    
    float xOffset = -50f;
    
    if (isCompleted)
    {
        var checkContainer = new CenterContainer();
        checkContainer.CustomMinimumSize = new Vector2(40, 64);
        checkContainer.Size = new Vector2(40, 64);
        checkContainer.Position = new Vector2(xOffset, 0);
        AddChild(checkContainer);
        
        var confirmScene = ResourceLoader.Load<PackedScene>("res://scenes/ui/confirm_button.tscn");
        if (confirmScene != null)
        {
            var tempButton = confirmScene.Instantiate<NConfirmButton>();
            var buttonImage = tempButton.GetNode<Control>("Image");
    
            if (buttonImage != null)
            {
                var icon = buttonImage.GetNode<TextureRect>("Icon");
                if (icon != null)
                {
                    _completionDot = icon.Duplicate(7) as Control;
                    _completionDot.CustomMinimumSize = new Vector2(28, 28);
                    _completionDot.Size = new Vector2(28, 28);
                    _completionDot.Scale = new Vector2(3f, 3f);
                    _completionDot.Modulate = new Color(0.3f, 1.0f, 0.3f);
                    checkContainer.AddChild(_completionDot);
                }
            }
    
            tempButton.QueueFree();
        }
        xOffset -= 65f;
    }
    
    if (isCompleted && highestAscension > 0)
    {
        var ascensionContainer = new CenterContainer();
        ascensionContainer.CustomMinimumSize = new Vector2(50, 64);
        ascensionContainer.Size = new Vector2(50, 64);
        ascensionContainer.Position = new Vector2(xOffset, -8);
        AddChild(ascensionContainer);
        
        _ascensionIndicator = CreateAscensionIndicator(highestAscension);
        ascensionContainer.AddChild(_ascensionIndicator);
    }
    
    _tickbox = new NTickbox();
    _tickbox.CustomMinimumSize = new Vector2(64, 64);
    _tickbox.MouseFilter = MouseFilterEnum.Stop;
    
    var tickboxScene = ResourceLoader.Load<PackedScene>("res://scenes/ui/tickbox.tscn");
    var tickboxVisuals = tickboxScene.Instantiate<Control>();
    tickboxVisuals.Name = "TickboxVisuals";
    tickboxVisuals.UniqueNameInOwner = true;
    _tickbox.AddChild(tickboxVisuals);
    tickboxVisuals.Owner = _tickbox;
    
    hbox.AddChild(_tickbox);
    
    _tickbox.Toggled += OnTickboxToggled;
    
    Callable.From(() => {
        _suppressEvents = true;
        _tickbox.IsTicked = false;
        _suppressEvents = false;
        _isSelected = false;
    }).CallDeferred();
    
    _nameLabel = new Label();
    _nameLabel.Text = challenge.Name;
    _nameLabel.AddThemeColorOverride("font_color", new Color(1f, 1f, 1f, 1f));
    _nameLabel.AddThemeFontSizeOverride("font_size", 34);
    _nameLabel.VerticalAlignment = VerticalAlignment.Center;
    
    var font = ResourceLoader.Load<FontVariation>("res://themes/kreon_bold_glyph_space_one.tres");
    if (font != null)
    {
        _nameLabel.AddThemeFontOverride("font", font);
    }
    
    _nameLabel.AddThemeColorOverride("font_shadow_color", new Color(0f, 0f, 0f, 0.501961f));
    _nameLabel.AddThemeConstantOverride("shadow_offset_x", 3);
    _nameLabel.AddThemeConstantOverride("shadow_offset_y", 2);
    
    hbox.AddChild(_nameLabel);
    
    CustomMinimumSize = new Vector2(400, 64);
}
    
    private Control CreateAscensionIndicator(int ascensionLevel)
    {
        var ascensionScene = ResourceLoader.Load<PackedScene>("res://scenes/screens/ascension_panel.tscn");
        if (ascensionScene == null) return CreateFallbackIndicator(ascensionLevel);
    
        var tempPanel = ascensionScene.Instantiate<NAscensionPanel>();
        tempPanel.Scale = new Vector2(0.5f, 0.5f);
    
        var iconContainer = tempPanel.GetNode<Control>("HBoxContainer/AscensionIconContainer");
    
        if (iconContainer == null)
        {
            tempPanel.QueueFree();
            return CreateFallbackIndicator(ascensionLevel);
        }
    
        var duplicate = iconContainer.Duplicate(8) as Control;
    
        var levelLabel = duplicate.GetNode<Label>("AscensionIcon/AscensionLevel");
        if (levelLabel != null)
        {
            levelLabel.Text = ascensionLevel.ToString();
            levelLabel.AddThemeFontSizeOverride("font_size", 24);
            levelLabel.AddThemeConstantOverride("outline_size", 12);
            levelLabel.AddThemeConstantOverride("shadow_offset_x", 3);
            levelLabel.AddThemeConstantOverride("shadow_offset_y", 3);
            levelLabel.Position = new Vector2(levelLabel.Position.X, levelLabel.Position.Y - 6);
        }
    
        tempPanel.QueueFree();
        return duplicate;
    }
    
    private Control CreateFallbackIndicator(int ascensionLevel)
    {
        var fallbackContainer = new PanelContainer();
        fallbackContainer.CustomMinimumSize = new Vector2(32, 32);
    
        var styleBox = new StyleBoxFlat();
        styleBox.BgColor = new Color(0.8f, 0.6f, 0.2f, 1.0f);
        styleBox.CornerRadiusTopLeft = 16;
        styleBox.CornerRadiusTopRight = 16;
        styleBox.CornerRadiusBottomLeft = 16;
        styleBox.CornerRadiusBottomRight = 16;
        fallbackContainer.AddThemeStyleboxOverride("panel", styleBox);
    
        var label = new Label();
        label.Text = ascensionLevel.ToString();
        label.HorizontalAlignment = HorizontalAlignment.Center;
        label.VerticalAlignment = VerticalAlignment.Center;
        label.AddThemeFontSizeOverride("font_size", 16);
        label.AddThemeColorOverride("font_color", new Color(0f, 0f, 0f, 1f));
    
        fallbackContainer.AddChild(label);
    
        return fallbackContainer;
    }
    
    private void OnTickboxToggled(NTickbox tickbox)
    {
        if (_suppressEvents) return;
        
        _isSelected = tickbox.IsTicked;
        Selected?.Invoke(this);
    }
    
    public void Deselect()
    {
        IsSelected = false;
    }
}