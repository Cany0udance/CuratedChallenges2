using CuratedChallenges.ChallengeUtil;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.HoverTips;

namespace CuratedChallenges.Patches.SpecificChallenges.VakuuTakeTheWheel;

[HarmonyPatch(typeof(NEndTurnButton), nameof(NEndTurnButton.Initialize))]
public static class VakuuAutoEndTogglePatch
{
    private static TextureButton _toggleButton;
    
    static void Postfix(NEndTurnButton __instance)
    {
        if (_toggleButton != null && GodotObject.IsInstanceValid(_toggleButton))
        {
            _toggleButton.QueueFree();
            _toggleButton = null;
        }
        
        if (!WhisperingEarringVakuuTakeTheWheelPatch.IsVakuuTakeTheWheelActive())
            return;
        
        var texture = GD.Load<Texture2D>("res://images/ui/run_history/vakuu.png");
        if (texture == null) return;
        
        _toggleButton = new TextureButton();
        _toggleButton.TextureNormal = texture;
        _toggleButton.CustomMinimumSize = new Vector2(48, 48);
        _toggleButton.StretchMode = TextureButton.StretchModeEnum.KeepAspectCentered;
        _toggleButton.Position = new Vector2(-80, 5);
        _toggleButton.Modulate = VakuuAutoEndState.IsEnabled
            ? Colors.White
            : new Color(1f, 1f, 1f, 0.4f);
        
        _toggleButton.Pressed += OnTogglePressed;
        
        _toggleButton.Connect("mouse_entered",
            Callable.From(() => OnHoverEntered(_toggleButton)));
        _toggleButton.Connect("mouse_exited",
            Callable.From(() => OnHoverExited(_toggleButton)));
        
        __instance.AddChild(_toggleButton);
    }
    
    private static void OnTogglePressed()
    {
        VakuuAutoEndState.IsEnabled = !VakuuAutoEndState.IsEnabled;
        
        if (_toggleButton != null)
        {
            _toggleButton.Modulate = VakuuAutoEndState.IsEnabled
                ? Colors.White
                : new Color(1f, 1f, 1f, 0.4f);
            
            // Refresh tooltip to reflect new state
            NHoverTipSet.Remove(_toggleButton);
            OnHoverEntered(_toggleButton);
        }
    }
    
    private static void OnHoverEntered(Control anchor)
    {
        var title = new LocString("gameplay_ui", "VAKUU_AUTO_END_TITLE");
        var description = new LocString("gameplay_ui", "VAKUU_AUTO_END_DESCRIPTION");
    
        var tip = new HoverTip(title, description);
    
        var tipSet = NHoverTipSet.CreateAndShow(anchor, new List<IHoverTip> { tip });
        tipSet?.SetGlobalPosition(anchor.GlobalPosition + new Vector2(-100, -220));
    }
    
    private static void OnHoverExited(Control anchor)
    {
        NHoverTipSet.Remove(anchor);
    }
}