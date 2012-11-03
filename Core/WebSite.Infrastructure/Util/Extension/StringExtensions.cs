using System;
using System.Text;

namespace Website.Infrastructure.Util.Extension
{
    public static class StringExtensions
    {
        public static String EmptyIfNull(this string source)
        {
            return source ?? string.Empty;
        }

        public static String ToLowerUnderScore(this string source)
        {
            if (source == null)
                return null;
            var stringBuilder = new StringBuilder();
            foreach (var c in source)
            {
                if (Char.IsLetterOrDigit(c) || c == '_')
                    stringBuilder.Append(Char.ToLower(c));
                else if (Char.IsWhiteSpace(c))
                    stringBuilder.Append('_');
            }
            return stringBuilder.ToString(); 
        }
    }
}
