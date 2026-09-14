using System.Text.RegularExpressions;

namespace Shikhsa.Helpers
{
    public static class TemplateMergeHelper
    {
        private static readonly Regex TokenPattern = new(@"\{\{([A-Za-z0-9_\.]+)\}\}", RegexOptions.Compiled);

        /// <summary>
        /// Returns every distinct {{Token}} found in the body, in the order they appear.
        /// e.g. "Student.FullName", "Certificate.TotalWorkingDays"
        /// </summary>
        public static List<string> ExtractTokens(string bodyHtml)
        {
            var tokens = new List<string>();
            foreach (Match m in TokenPattern.Matches(bodyHtml ?? string.Empty))
            {
                var token = m.Groups[1].Value;
                if (!tokens.Contains(token))
                {
                    tokens.Add(token);
                }
            }
            return tokens;
        }

        /// <summary>
        /// Replaces every {{Token}} in bodyHtml with values[token], if present.
        /// Tokens with no matching value are replaced with an empty string
        /// (rather than left as raw "{{...}}" text in the printed certificate).
        /// </summary>
        public static string Merge(string bodyHtml, Dictionary<string, string> values)
        {
            return TokenPattern.Replace(bodyHtml ?? string.Empty, match =>
            {
                var token = match.Groups[1].Value;
                return values.TryGetValue(token, out var value) ? (value ?? string.Empty) : string.Empty;
            });
        }
    }
}
