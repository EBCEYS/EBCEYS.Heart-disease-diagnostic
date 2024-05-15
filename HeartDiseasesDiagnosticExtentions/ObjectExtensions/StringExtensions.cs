using System.Text.Json;
using System.Text.Json.Nodes;

namespace HeartDiseasesDiagnosticExtentions.ObjectExtensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// Checks that string is json.
        /// </summary>
        /// <param name="s">The string.</param>
        /// <returns><c>true</c> if string is json; otherwise <c>false</c>.</returns>
        public static bool IsJson(this string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return false;
            }
            try
            {
                JsonNode.Parse(s);
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }

        /// <summary>
        /// Checks that string is json and is empty json.
        /// </summary>
        /// <param name="s">The string.</param>
        /// <returns><c>true</c> if string is empty json "{}" or not json; otherwise <c>false</c>.</returns>
        public static bool IsEmptyJson(this string s)
        {
            return IsJson(s) && string.Compare(s, "{}", true) == 0;
        }
    }
}
