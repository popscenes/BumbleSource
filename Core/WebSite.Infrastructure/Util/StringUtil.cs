using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    }
}
