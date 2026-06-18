using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using BaseLib.Config;
using CuratedChallenges.ChallengeUtil.Progression;
using Godot;
using MegaCrit.Sts2.Core.Entities.UI;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;
using MegaCrit.Sts2.Core.Saves;

namespace CuratedChallenges.CuratedChallengesCode;

public class CuratedChallengesConfig : SimpleModConfig
{
    public static int DefaultAscension { get; set; } = 0;
    
    [ConfigHideInUI]
    [ConfigIgnoreRestoreDefaults]
    public static string CompletedChallengesJson { get; set; } = "{}";

    private static Dictionary<string, int>? _completedCache;
    private static bool _migrationAttempted;

    public static Dictionary<string, int> GetCompletedChallenges()
    {
        if (!_migrationAttempted)
        {
            _migrationAttempted = true;
            MigrateLegacyData();
        }

        if (_completedCache == null)
        {
            _completedCache = JsonSerializer.Deserialize<Dictionary<string, int>>(CompletedChallengesJson)
                              ?? new Dictionary<string, int>();
        }
        return _completedCache;
    }
    
    private static void MigrateLegacyData()
    {
        try
        {
            string path = SaveManager.Instance.GetProfileScopedPath("challenge_data.json");

            if (!Godot.FileAccess.FileExists(path))
                return;

            using var file = Godot.FileAccess.Open(path, Godot.FileAccess.ModeFlags.Read);
            string json = file.GetAsText();

            var legacy = JsonSerializer.Deserialize<ChallengeRunData>(json, new JsonSerializerOptions
            {
                TypeInfoResolver = new DefaultJsonTypeInfoResolver()
            });

            if (legacy?.CompletedChallenges == null || legacy.CompletedChallenges.Count == 0)
            {
                Godot.DirAccess.RemoveAbsolute(path);
                return;
            }

            var current = GetCompletedChallenges();

            foreach (var (key, ascension) in legacy.CompletedChallenges)
            {
                if (!current.TryGetValue(key, out int existing) || ascension > existing)
                {
                    current[key] = ascension;
                }
            }

            SaveCompletedChallenges();
            Godot.DirAccess.RemoveAbsolute(path);
            GD.Print($"[CuratedChallenges] Migrated {legacy.CompletedChallenges.Count} entries from legacy file.");
        }
        catch (Exception e)
        {
            GD.PrintErr($"[CuratedChallenges] Migration failed: {e}");
        }
    }

    public static void SaveCompletedChallenges()
    {
        if (_completedCache == null) return;

        CompletedChallengesJson = JsonSerializer.Serialize(_completedCache);
        ModConfig.SaveDebounced<CuratedChallengesConfig>();
    }
    
    public override void SetupConfigUI(Control optionContainer)
    {
        var hbox = new HBoxContainer();
        hbox.AddThemeConstantOverride("separation", 10);
        hbox.CustomMinimumSize = new Vector2(0f, 64f);
        optionContainer.AddChild(hbox);
        
        var label = CreateRawLabelControl(
            new LocString("settings_ui", "CURATEDCHALLENGES.default_ascension").GetFormattedText(), 
            28);
        label.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        hbox.AddChild(label);
        
        var valueLabel = CreateRawLabelControl(DefaultAscension.ToString(), 28);
        valueLabel.CustomMinimumSize = new Vector2(40f, 0f);
        valueLabel.HorizontalAlignment = HorizontalAlignment.Right;
        
        var slider = new HSlider();
        slider.MinValue = 0;
        slider.MaxValue = 10;
        slider.Step = 1;
        slider.Value = DefaultAscension;
        slider.CustomMinimumSize = new Vector2(200f, 32f);
        slider.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        slider.SizeFlagsVertical = Control.SizeFlags.ShrinkCenter;
        
        slider.ValueChanged += (double val) =>
        {
            DefaultAscension = (int)val;
            valueLabel.Text = DefaultAscension.ToString();
            Changed();
            SaveDebounced();
        };
        
        hbox.AddChild(slider);
        hbox.AddChild(valueLabel);
        
        AddRestoreDefaultsButton(optionContainer);
    }
}