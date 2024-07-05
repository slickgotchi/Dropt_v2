namespace Dropt.Utils
{
    public class Color
    {

        public static UnityEngine.Color HexToColor(string hex)
        {
            // Remove the '#' character if it's present
            hex = hex.Replace("#", "");

            // Parse the r, g, b, and a components from the hex string
            byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
            byte a = 255; // Default to fully opaque if alpha is not specified in the hex

            // If the hex includes alpha component (8 characters long)
            if (hex.Length == 8)
            {
                a = byte.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
            }

            // Convert to [0, 1] range and create Color object
            return new UnityEngine.Color(r / 255f, g / 255f, b / 255f, a / 255f);
        }


    }
}
