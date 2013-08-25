using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Website.Infrastructure.Util.Extension
{
    public static class StringExtensions
    {
        public static String EmptyIfNull(this string source)
        {
            return source ?? string.Empty;
        }

        public static String ToLowerHiphen(this string source)
        {
            if (source == null)
                return null;
            var stringBuilder = new StringBuilder();
            foreach (var c in source)
            {
                if (Char.IsLetterOrDigit(c) || c == '-')
                    stringBuilder.Append(Char.ToLower(c));
                else if (Char.IsWhiteSpace(c))
                    stringBuilder.Append('-');
            }
            return stringBuilder.ToString(); 
        }

        public static EnumType AsEnum<EnumType>(this string source, EnumType @default = default(EnumType)) where EnumType : struct
        {
            EnumType ret;
            return Enum.TryParse(source, true, out ret) ? ret : @default;
        }

        public static String ToLetterOrDigitAndSpaceOnly(this string source)
        {
            if (source == null)
                return null;
            var stringBuilder = new StringBuilder();
            foreach (var c in source)
            {
                if (Char.IsLetterOrDigit(c) || c == '-')
                    stringBuilder.Append(c);
                else 
                    stringBuilder.Append(' ');
            }
            return stringBuilder.ToString(); 
        }

        public static string Quoted(this string source)
        {
            return @"""" + source + @"""";
        }

        private static readonly HashSet<string> IgnoreWords = new HashSet<string>()
            {
                "and", "the" 
            }; 
        public static List<string> TokenizeMeaningfulWordsAndSort(this string source)
        {
            var words = Regex.Split(source, @"\W");
            return
                words
                    .Select(s => s.ToLower())
                    .Where(w => w.Length > 2 && !IgnoreWords.Contains(w))
                    .OrderBy(s => s)
                    .ToList();
        }
    }
}
