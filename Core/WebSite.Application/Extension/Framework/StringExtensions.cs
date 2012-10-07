using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Website.Application.Extension.Framework
{
    public static class StringExtensions
    {
        public static String EmptyIfNull(this string source)
        {
            return source ?? string.Empty;
        }
    }
}
