using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace EventRegistrator.Functions.Payments
{
    public class EmailExtractor
    {
        private static readonly Regex Email = new Regex("");

        public static IEnumerable<string> TryExtractEmailFromInfo(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return null;
            }
            var matches = Email.Matches(input);
            return matches.OfType<Match>().Select(mat => mat.Value);
        }
    }
}