using UnityEngine;
using System.Text;
using System.Text.RegularExpressions;

namespace Dropt.Utils
{
    public static class String
    {
        public static string ConvertToReadableString(string input)
        {
            // Use a StringBuilder for efficient string manipulation
            StringBuilder result = new StringBuilder();

            // Add a space before each uppercase letter (except if it's the first character)
            foreach (char c in input)
            {
                if (char.IsUpper(c) && result.Length > 0 && result[result.Length - 1] != ' ')
                {
                    result.Append(' ');
                }

                // Replace underscores, hyphens with space
                if (c == '_' || c == '-')
                {
                    result.Append(' ');
                }
                // Add a space before numbers if the previous character is a letter
                else if (char.IsDigit(c) && result.Length > 0 && char.IsLetter(result[result.Length - 1]))
                {
                    result.Append(' ');
                    result.Append(c);
                }
                else
                {
                    result.Append(c);
                }
            }

            // Convert to string and trim any leading/trailing spaces
            string formattedString = result.ToString().Trim();

            // Capitalize the first letter of each word
            formattedString = Regex.Replace(formattedString, @"(^\w)|(\s\w)", m => m.Value.ToUpper());

            return formattedString;
        }
    }
}
