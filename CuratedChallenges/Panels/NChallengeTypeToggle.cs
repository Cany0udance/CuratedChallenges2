using Godot;
using MegaCrit.Sts2.Core.Logging;

namespace CuratedChallenges.Panels;

public class NChallengeTypeToggle : Control
{
    public enum ChallengeType
    {
        Individual,
        Shared
    }
    
    public delegate void TypeChangedDelegate(ChallengeType type);
    public event TypeChangedDelegate TypeChanged;
    
    private ChallengeType _currentType = ChallengeType.Individual;
    private Button _individualButton;
    private Button _sharedButton;
    
    private static readonly Color SELECTED_COLOR = new Color(0.8f, 0.8f, 1.0f, 1.0f);
    private static readonly Color UNSELECTED_COLOR = new Color(0.4f, 0.4f, 0.4f, 1.0f);
    
    public ChallengeType CurrentType => _currentType;
    
    public NChallengeTypeToggle()
    {
        Initialize();
    }
    
    private void Initialize()
    {
        var container = new HBoxContainer();
        container.AddThemeConstantOverride("separation", 10);
        AddChild(container);
        
        _individualButton = new Button();
        _individualButton.Text = "Individual";
        _individualButton.CustomMinimumSize = new Vector2(120, 40);
        _individualButton.Pressed += () => SetType(ChallengeType.Individual);
        container.AddChild(_individualButton);
        
        _sharedButton = new Button();
        _sharedButton.Text = "Shared";
        _sharedButton.CustomMinimumSize = new Vector2(120, 40);
        _sharedButton.Pressed += () => SetType(ChallengeType.Shared);
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