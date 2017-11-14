using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace EventRegistrator.Functions.Payments
{
    public static class EmailExtractor
    {
        private static readonly Regex Email = new Regex(@"[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?");

        public static IEnumerable<string> TryExtractEmailFromInfo(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return null;
            }
            var matches = Email.Matches(input?.ToLowerInvariant());
            return matches.OfType<Match>().Select(mat => mat.Value?.ToLowerInvariant());
        }
    }
}