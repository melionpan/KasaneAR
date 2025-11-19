using UnityEngine;
using System.Collections.Generic;

public static class ColorMixingRules
{
    // Color mixing rules
    private static readonly Dictionary<(Color, Color), Color> MixingRules = new()
    {
        {(Color.red, Color.blue), new Color(0.5f, 0f, 0.5f, 1f)},     // Purple
        {(Color.blue, Color.red), new Color(0.5f, 0f, 0.5f, 1f)},     // Purple
        
        {(Color.red, Color.yellow), new Color(1f, 0.5f, 0f, 1f)},     // Orange
        {(Color.yellow, Color.red), new Color(1f, 0.5f, 0f, 1f)},     // Orange
        
        {(Color.blue, Color.yellow), Color.green},                              // Green
        {(Color.yellow, Color.blue), Color.green},                              // Green
        
        {(Color.green, Color.red), new Color(0.5f, 0.25f, 0f, 1f)},   // Brown
        {(Color.red, Color.green), new Color(0.5f, 0.25f, 0f, 1f)},   // Brown
    };

    public static Color MixColors(Color color1, Color color2)
    {
        // Check if we have a specific mixing rule
        var key = (color1, color2);
        if (MixingRules.TryGetValue((color1, color2), out Color result))
        {
            return result;
        }

        // Fallback: average the colors
        return (color1 + color2) * 0.5f;
    }

    public static bool CanMix(Color color1, Color color2)
    {
        // Colors can mix if they're different and not white
        return color1 != Color.white && 
               color2 != Color.white && 
               color1 != color2;
    }
}