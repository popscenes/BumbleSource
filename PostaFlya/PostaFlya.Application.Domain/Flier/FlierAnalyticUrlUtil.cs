using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PostaFlya.Domain.Flier.Analytic;
using PostaFlya.Domain.Flier.Command;

namespace PostaFlya.Application.Domain.Flier
{

    public static class FlierAnalyticUrlUtil
    {
        public static FlierAnalyticSourceAction GetSourceActionFromParam(string param, FlierAnalyticSourceAction defaultVal)
        {
            if (string.IsNullOrWhiteSpace(param))
                return defaultVal;
            FlierAnalyticSourceAction ret;
            return Enum.TryParse(param, out ret) ? ret : defaultVal;
        }

        private const string Format = "q={0}";
        public static string AddAnalyticString(this string sourceUrl, FlierAnalyticSourceAction action)
        {
            return (sourceUrl.Contains("?"))
                       ? sourceUrl + "&" + string.Format(Format, (int) action)
                       : sourceUrl + "?" + string.Format(Format, (int)action);
        }

    }
}
