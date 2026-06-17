using Godot;

namespace CuratedChallenges.ChallengeUtil.DetailsPanel;

/// <summary>
/// Factory for creating consistently styled labels used in the Challenge Details Panel.
/// Handles font loading, color schemes, and common label configurations.
/// All labels created through this factory share the same visual style (fonts, colors, shadows).
/// </summary>
public class ChallengeDetailsPanelLabelFactory
{
    private const string REGULAR_FONT_PATH = "res://themes/kreon_regular_glyph_space_one.tres";
    private const string BOLD_FONT_PATH = "res://themes/kreon_bold_glyph_space_one.tres";
    
    private static readonly Color GOLD_COLOR = new Color(1f, 0.84f, 0f, 1f);
    private static readonly Color YELLOW_COLOR = new Color(1f, 1f, 0f, 1f);
    private static readonly Color WHITE_COLOR = new Color(1f, 1f, 1f, 1f);
    private static readonly Color PURPLE_COLOR = new Color(0.627f, 0.125f, 0.941f, 1f); // Purple for enchantments
    private static readonly Color SHADOW_COLOR = new Color(0f, 0f, 0f, 0.501961f);
    
    private FontVariation _regularFont;
    private FontVariation _boldFont;
    
    public ChallengeDetailsPanelLabelFactory()
    {
        LoadFonts();
    }
    
    private void LoadFonts()
    {
        _regularFont = ResourceLoader.Load<FontVariation>(REGULAR_FONT_PATH);
        _boldFont = ResourceLoader.Load<FontVariation>(BOLD_FONT_PATH);
    }
    
    public Label CreateHeaderLabel(string text)
    {
        var label = new Label();
        label.Text = text;
        label.AddThemeColorOverride("font_color", GOLD_COLOR);
        label.AddThemeFontSizeOverride("font_size", 34);
        
        if (_boldFont != null)
        {
            label.AddThemeFontOverride("font", _boldFont);
        }
        
        label.AddThemeColorOverride("font_shadow_color", SHADOW_COLOR);
        label.AddThemeConstantOverride("shadow_offset_x", 3);
        label.AddThemeConstantOverride("shadow_offset_y", 2);
        
        return label;
    }
    
    public Label CreateContentLabel()
    {
        var label = new Label();
        label.AddThemeColorOverride("font_color", YELLOW_COLOR);
        label.AddThemeFontSizeOverride("font_size", 20);
        
        if (_regularFont != null)
        {
            label.AddThemeFontOverride("font", _regularFont);
        }
        
        label.CustomMinimumSize = new Vector2(400, 0);
        return label;
    }
    
    public Label CreatePlainLabel(string text)
    {
        var label = new Label();
        label.Text = text;
        label.AddThemeColorOverride("font_color", WHITE_COLOR);
        label.AddThemeFontSizeOverride("font_size", 20);
        
        if (_regularFont != null)
        {
            label.AddThemeFontOverride("font", _regularFont);
        }
        
        return label;
    }
    
    public Label CreateColoredLabel(string text, Color? color)
    {
        var label = new Label();
        label.Text = text;
        
        Color finalColor = color ?? WHITE_COLOR;
        label.AddThemeColorOverride("font_color", finalColor);
        label.AddThemeFontSizeOverride("font_size", 20);
        
        if (_regularFont != null)
        {
            label.AddThemeFontOverride("font", _regularFont);
        }
        
        return label;
    }
    
    public Label CreateKeywordLabel(string keyword)
    {
        var label = new Label();
        label.Text = keyword;
        label.AddThemeColorOverride("font_color", GOLD_COLOR);
        label.AddThemeFontSizeOverride("font_size", 20);
        
        if (_regularFont != null)
        {
            label.AddThemeFontOverride("font", _regularFont);
        }
        
        label.MouseFilter = Control.MouseFilterEnum.Stop;
        
        return label;
    }
    
    public Label CreateCardNameLabel(string cardName, Color? textColor)
    {
        var label = new Label();
        label.Text = cardName;
        
        Color finalColor = textColor ?? WHITE_COLOR;
        label.AddThemeColorOverride("font_color", finalColor);
        label.AddThemeFontSizeOverride("font_size", 20);
        
        if (_regularFont != null)
        {
            label.AddThemeFontOverride("font", _regularFont);
        }
        
        label.MouseFilter = Control.MouseFilterEnum.Stop;
        
        return label;
    }
    
    public Label CreateRelicNameLabel(string relicName, Color? textColor)
    {
        var label = new Label();
        label.Text = relicName;
    
        Color finalColor = textColor ?? WHITE_COLOR;
        label.AddThemeColorOverride("font_color", finalColor);
        label.AddThemeFontSizeOverride("font_size", 20);
    
        if (_regularFont != null)
        {
            label.AddThemeFontOverride("font", _regularFont);
        }
    
        label.MouseFilter = Control.MouseFilterEnum.Stop;
    
        return label;
    }
    
    public Label CreateEnchantmentNameLabel(string enchantmentName, Color? textColor)
    {
        var label = new Label();
        label.Text = enchantmentName;
    
        Color finalColor = textColor ?? PURPLE_COLOR;
        label.AddThemeColorOverride("font_color", finalColor);
        label.AddThemeFontSizeOverride("font_size", 20);
    
        if (_regularFont != null)
        {
            label.AddThemeFontOverride("font", _regularFont);
        }
    
        label.MouseFilter = Control.MouseFilterEnum.Stop;
    
        return label;
    }
    
    public Label CreatePotionNameLabel(string potionName, Color? textColor)
    {
        var label = new Label();
        label.Text = potionName;

        Color finalColor = textColor ?? WHITE_COLOR;
        label.AddThemeColorOverride("font_color", finalColor);
        label.AddThemeFontSizeOverride("font_size", 20);

        if (_regularFont != null)
        {
            label.AddThemeFontOverride("font", _regularFont);
        }

        label.MouseFilter = Control.MouseFilterEnum.Stop;

        return label;
    }
}