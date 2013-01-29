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
        public const string QrSourceParam = "q";
        public static FlierAnalyticSourceAction GetSourceActionFromParam(string param, FlierAnalyticSourceAction defaultVal)
        {
            if (string.IsNullOrWhiteSpace(param))
                return defaultVal;
            FlierAnalyticSourceAction ret;
            return Enum.TryParse(param, out ret) ? ret : defaultVal;
        }

        public static FlierAnalyticSourceAction GetSourceAction(string url, FlierAnalyticSourceAction defaultVal)
        {
            if (string.IsNullOrWhiteSpace(url))
                return defaultVal;

            var source = new Uri(url);
            var param = System.Web.HttpUtility.ParseQueryString(source.Query);
            var val = param.Get(QrSourceParam);
            return GetSourceActionFromParam(val, defaultVal);
        }

        private const string Format = QrSourceParam + "={0}";
        public static string AddAnalyticString(this string sourceUrl, FlierAnalyticSourceAction action)
        {
            return (sourceUrl.Contains("?"))
                       ? sourceUrl + "&" + string.Format(Format, (int) action)
                       : sourceUrl + "?" + string.Format(Format, (int)action);
        }

    }
}
