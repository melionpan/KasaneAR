using UnityEngine;
using System.Collections.Generic;

public static class ColorMixingRules
{
    // Color mixing rules
    private static readonly Dictionary<(Color, Color), Color> MixingRules = new()
    {
        // Red + Blue = PURPLE/
        {(Color.red, Color.blue), new Color(0.6f, 0.1f, 0.8f, 1f)},
        {(Color.blue, Color.red), new Color(0.6f, 0.1f, 0.8f, 1f)},
        
        // Red + Yellow = ORANGE
        {(Color.red, Color.yellow), new Color(1f, 0.65f, 0f, 1f)},
        {(Color.yellow, Color.red), new Color(1f, 0.65f, 0f, 1f)},
        
        // Red + Green = BROWN
        {(Color.green, Color.red), new Color(0.6f, 0.3f, 0.1f, 1f)},
        {(Color.red, Color.green), new Color(0.6f, 0.3f, 0.1f, 1f)},
        
        // Blue + Yellow = GREEN
        {(Color.blue, Color.yellow), new Color(0.1f, 0.7f, 0.2f, 1f)},
        {(Color.yellow, Color.blue), new Color(0.1f, 0.7f, 0.2f, 1f)},
        
        // Blue + Green = TEAL
        {(Color.blue, Color.green), new Color(0f, 0.8f, 0.8f, 1f)},
        {(Color.green, Color.blue), new Color(0f, 0.8f, 0.8f, 1f)},
        
        // Green + Yellow = LIME GREEN
        {(Color.green, Color.yellow), new Color(0.7f, 1f, 0.2f, 1f)},
        {(Color.yellow, Color.green), new Color(0.7f, 1f, 0.2f, 1f)},

        
        // === WHITE COLORED MIX ===
        // White + Red = SOFT PINK
        {(Color.white, Color.red), new Color(1f, 0.5f, 0.8f, 1f)},
        {(Color.red, Color.white), new Color(1f, 0.5f, 0.8f, 1f)},
        
        // White + Blue = LIGHT BLUE
        {(Color.white, Color.blue), new Color(0.2f, 0.85f, 1f, 1f)},
        {(Color.blue, Color.white), new Color(0.2f, 0.85f, 1f, 1f)},
        
        // White + Green = LIGHT GREEN
        {(Color.white, Color.green), new Color(0.5f, 1f, 0.7f, 1f)},
        {(Color.green, Color.white), new Color(0.5f, 1f, 0.7f, 1f)},

        // White + Yellow = LIGHT YELLOW
        {(Color.white, Color.yellow), new Color(1f, 1f, 0.6f, 1f)},
        {(Color.yellow, Color.white), new Color(1f, 1f, 0.6f, 1f)},

        
        // === SAME COLORED MIX ===
        // Red + Red = RED
        {(Color.red, Color.red), Color.red},
        
        // Blue + Blue = BLUE
        {(Color.blue, Color.blue), Color.blue}, 
        
        // Green + Green = BRIGHT GREEN
        {(Color.green, Color.green), Color.green}, 
        
        // Yellow + Yellow = YELLOW
        {(Color.yellow, Color.yellow), Color.yellow}, 

        // White + White = WHITE
        {(Color.white, Color.white), Color.white}, 
        
    };

    public static Color MixColors(Color color1, Color color2)
    {
        // Check if we have a specific mixing rule
        if (MixingRules.TryGetValue((color1, color2), out Color result))
        {
            return result;
        }

        // Fallback: average the colors
        return (color1 + color2) * 0.5f;
    }
}