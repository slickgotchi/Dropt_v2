using UnityEngine;

namespace Dropt.Utils
{
    public static class Color
    {
        public static UnityEngine.Color HexToColor(string hex, float alpha = 1.0f)
        {
            if (!ColorUtility.TryParseHtmlString(hex, out UnityEngine.Color color))
            {
                Debug.LogError("Invalid hex color string: " + hex);
                return UnityEngine.Color.black; // Default to black if the input is invalid
            }

            // Ensure alpha is clamped between 0 and 1
            alpha = Mathf.Clamp01(alpha);

            // Apply the alpha value to the color
            color.a = alpha;

            return color;
        }
    }
}
