using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Website.Infrastructure.Util
{
    public static class StringUtil
    {
        public static bool AreBothEqualOrNullOrWhiteSpace(string one, string two, bool ignorecase = true)
        {
            if (one == two)
                return true;

            if (string.IsNullOrWhiteSpace(one) && string.IsNullOrWhiteSpace(two))
                return true;
            if (string.IsNullOrWhiteSpace(one) || string.IsNullOrWhiteSpace(two))
                return false;

            return one.Equals(two, ignorecase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);

        }

        public static string[] SplitStringTrimRemoveEmpty(this string original, char separator = ',')
        {
            if (String.IsNullOrEmpty(original))
            {
                return new string[0];
            }

            var split = from piece in original.Split(separator)
                        let trimmed = piece.Trim()
                        where !String.IsNullOrEmpty(trimmed)
                        select trimmed;
            return split.ToArray();
        }


        public static decimal LevenshteinDistanceAsPercentage(this string source, string to)
        {
            if (string.IsNullOrWhiteSpace(source) && string.IsNullOrWhiteSpace(to))
                return 0;
            if (string.IsNullOrWhiteSpace(source))
                return 100;

            var dist = source.LevenshteinDistanceTo(to);

            return Math.Abs((((decimal)dist)/source.Length)*100);
        }

        public static int LevenshteinDistanceTo(this string source, string to)
        {
            var n = source.Length;
            var m = to.Length;
            var d = new int[n + 1, m + 1];

            // Step 1
            if (n == 0)
            {
                return m;
            }

            if (m == 0)
            {
                return n;
            }

            // Step 2
            for (var i = 0; i <= n; d[i, 0] = i++)
            {
            }

            for (var j = 0; j <= m; d[0, j] = j++)
            {
            }

            // Step 3
            for (var i = 1; i <= n; i++)
            {
                //Step 4
                for (var j = 1; j <= m; j++)
                {
                    // Step 5
                    var cost = (to[j - 1] == source[i - 1]) ? 0 : 1;

                    // Step 6
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }
            // Step 7
            return d[n, m];
        }

        private static readonly HashSet<string> IgnoreWords = new HashSet<string>()
            {
                "and", "the" 
            };
        public static List<string> TokenizeMeaningfulWords(this string source)
        {
            var words = Regex.Split(source, @"\W");
            return
                words
                    .Select(s => s.ToLower())
                    .Where(w => w.Length > 2 && !IgnoreWords.Contains(w))
                    .ToList();
        }
    }
}
