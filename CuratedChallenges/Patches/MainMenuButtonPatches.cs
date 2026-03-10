using System.Reflection;
using CuratedChallenges.Screens;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;

namespace CuratedChallenges.MainMenu;
[HarmonyPatch(typeof(NSingleplayerSubmenu), "_Ready")]
public class AddChallengesButtonPatch
{
    private const string LOG_TAG = "[ChallengesButton]";
    private static NSubmenuButton _challengesButton;
    
    public static void Postfix(NSingleplayerSubmenu __instance)
    {
        try
        {
            var standardButtonField = typeof(NSingleplayerSubmenu).GetField("_standardButton",
                BindingFlags.NonPublic | BindingFlags.Instance);
            var dailyButtonField = typeof(NSingleplayerSubmenu).GetField("_dailyButton",
                BindingFlags.NonPublic | BindingFlags.Instance);
            var customButtonField = typeof(NSingleplayerSubmenu).GetField("_customButton",
                BindingFlags.NonPublic | BindingFlags.Instance);
                
            var standardButton = (NSubmenuButton)standardButtonField.GetValue(__instance);
            var dailyButton = (NSubmenuButton)dailyButtonField.GetValue(__instance);
            var customButton = (NSubmenuButton)customButtonField.GetValue(__instance);
            
            var spacing = ((Control)dailyButton).Position.X - ((Control)standardButton).Position.X;
            var yPosition = standardButton.Position.Y;
            var centerOffset = spacing / 2;
            
            standardButton.Position = new Vector2(standardButton.Position.X - centerOffset, yPosition);
            dailyButton.Position = new Vector2(standardButton.Position.X + spacing * 2, yPosition);
            ((Control)customButton).Position = new Vector2(standardButton.Position.X + spacing * 3, yPosition);
            
            // Duplicate with default flags (includes signals, which causes warnings but works)
            _challengesButton = (NSubmenuButton)customButton.Duplicate();
            _challengesButton.Name = "ChallengesButton";
            
            __instance.AddChild(_challengesButton);
            
            // Don't call ConnectSignals() - signals are already connected from Duplicate()
            
            // Duplicate the material so it's not shared
            var bgPanel = _challengesButton.GetNode<Control>((NodePath)"BgPanel");
            bgPanel.Material = (ShaderMaterial)bgPanel.Material.Duplicate();
            
            var hsvField = typeof(NSubmenuButton).GetField("_hsv",
                BindingFlags.NonPublic | BindingFlags.Instance);
            hsvField?.SetValue(_challengesButton, bgPanel.Material);
            
            var hsvMaterial = (ShaderMaterial)bgPanel.Material;
            hsvMaterial.SetShaderParameter("h", (Variant)0.83f); // adjust to change color of challenge ribbon
            
            _challengesButton.Position = new Vector2(standardButton.Position.X + spacing, yPosition);
            
            // Connect our click handler
            _challengesButton.Connect(
                NClickableControl.SignalName.Released,
                Callable.From(new Action<NButton>(_ => OpenChallengesScreen(__instance)))
            );
            
            _challengesButton.SetIconAndLocalization("CHALLENGES");
            
            var icon = _challengesButton.GetNode<TextureRect>((NodePath)"Icon");
            var iconTexture = ResourceLoader.Load<Texture2D>("res://images/packed/main_menu/submenu_icon_challenges.png");
            if (iconTexture != null)
            {
                icon.Texture = iconTexture;
            }
        }
        catch (Exception e)
        {
            Log.Error($"{LOG_TAG} Failed to add challenges button: {e}");
        }
    }
    
    private static void OpenChallengesScreen(NSingleplayerSubmenu instance)
    {
        var stackField = typeof(NSubmenu).GetField("_stack", 
            BindingFlags.NonPublic | BindingFlags.Instance);
        var stack = stackField?.GetValue(instance);
    
        if (stack != null)
        {
            var challengesScreen = new NChallengesScreen();
            ((Control)stack).AddChild(challengesScreen);
            
            var pushMethod = stack.GetType().GetMethod("Push");
            pushMethod?.Invoke(stack, new object[] { challengesScreen });
        }
    }
    
    public static NSubmenuButton GetChallengesButton()
    {
        return _challengesButton;
    }
}

[HarmonyPatch(typeof(NSingleplayerSubmenu), "RefreshButtons")]
public class RefreshButtonsPatch
{
    public static void Postfix()
    {
        var challengesButton = AddChallengesButtonPatch.GetChallengesButton();
        challengesButton?.SetEnabled(true);
    }
}