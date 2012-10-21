using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Website.Domain.Location;
using Website.Infrastructure.Query;

namespace PostaFlya.Domain.Flier.Query
{
    public static class FlierQueryServiceUtil
    {

        public static string FindFreeId(this GenericQueryServiceInterface queryService, FlierInterface newflier)
        {
            const string pattern = "@dd-MMM-yy";

            var title = newflier.Title;
            var stringBuilder = new StringBuilder();
            foreach (var c in title)
            {
                if (Char.IsLetterOrDigit(c) || c == '_')
                    stringBuilder.Append(Char.ToLower(c));
                else if(Char.IsWhiteSpace(c))
                    stringBuilder.Append('_');
            }
            var tryTitleBase = stringBuilder + newflier.EffectiveDate.ToString(pattern);
            var tryTitle = tryTitleBase;

            if (queryService != null && queryService.FindById<Flier>(tryTitle) != null)
            {
                if (newflier.Location.HasAddressParts())
                {
                    var locInfo = newflier.Location.Locality;
                    if (string.IsNullOrWhiteSpace(locInfo))
                        locInfo = newflier.Location.PostCode;
                    if (!string.IsNullOrWhiteSpace(locInfo))
                    {
                        tryTitleBase = locInfo + "_" + tryTitleBase;
                        tryTitle = tryTitleBase;                        
                    }
                }
            }
            else
                return tryTitle;


            var counter = 0;
            while (queryService != null && queryService.FindById<Flier>(tryTitle) != null)
            {
                tryTitle = (counter++) + "_" + tryTitleBase;
            }
            return tryTitle;
        }
    }
}
