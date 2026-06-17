using Godot;
using MegaCrit.Sts2.Core.Logging;

namespace CuratedChallenges.Panels;

public class ChallengeTypeToggle
{
    public enum ChallengeType
    {
        Individual,
        Shared
    }
    
    public delegate void TypeChangedDelegate(ChallengeType type);
    public event TypeChangedDelegate TypeChanged;
    
    public Control Root { get; }
    public ChallengeType CurrentType => _currentType;
    
    private ChallengeType _currentType = ChallengeType.Individual;
    private Button _individualButton;
    private Button _sharedButton;
    
    private static readonly Color SELECTED_COLOR = new Color(0.8f, 0.8f, 1.0f, 1.0f);
    private static readonly Color UNSELECTED_COLOR = new Color(0.4f, 0.4f, 0.4f, 1.0f);
    
    public ChallengeTypeToggle()
    {
        Root = new Control();
        
        var font = ResourceLoader.Load<Font>("res://fonts/kreon_bold.ttf");
        
        var container = new HBoxContainer();
        container.AddThemeConstantOverride("separation", 10);
        Root.AddChild(container);
        
        _individualButton = new Button();
        _individualButton.Text = "Individual";
        _individualButton.CustomMinimumSize = new Vector2(120, 40);
        _individualButton.Pressed += () => SetType(ChallengeType.Individual);
        if (font != null)
        {
            _individualButton.AddThemeFontOverride("font", font);
        }
        container.AddChild(_individualButton);
        
        _sharedButton = new Button();
        _sharedButton.Text = "Shared";
        _sharedButton.CustomMinimumSize = new Vector2(120, 40);
        _sharedButton.Pressed += () => SetType(ChallengeType.Shared);
        if (font != null)
        {
            _sharedButton.AddThemeFontOverride("font", font);
        }
        container.AddChild(_sharedButton);
        
        UpdateButtonStates();
    }
    
    public void SetType(ChallengeType type)
    {
        if (_currentType == type) return;
        
        _currentType = type;
        UpdateButtonStates();
        TypeChanged?.Invoke(_currentType);
    }
    
    private void UpdateButtonStates()
    {
        if (_individualButton == null || _sharedButton == null) return;
        
        _individualButton.Modulate = _currentType == ChallengeType.Individual 
            ? SELECTED_COLOR 
            : UNSELECTED_COLOR;
            
        _sharedButton.Modulate = _currentType == ChallengeType.Shared 
            ? SELECTED_COLOR 
            : UNSELECTED_COLOR;
    }
}