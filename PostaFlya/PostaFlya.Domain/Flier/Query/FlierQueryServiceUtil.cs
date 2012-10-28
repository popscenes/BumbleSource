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

        public static string FindFreeFriendlyId(this GenericQueryServiceInterface queryService, FlierInterface targetFlier)
        {
            const string pattern = "@dd-MMM-yy";

            var title = targetFlier.Title;
            var stringBuilder = new StringBuilder();
            foreach (var c in title)
            {
                if (Char.IsLetterOrDigit(c) || c == '_')
                    stringBuilder.Append(Char.ToLower(c));
                else if(Char.IsWhiteSpace(c))
                    stringBuilder.Append('_');
            }
            var tryTitleBase = stringBuilder + targetFlier.EffectiveDate.ToString(pattern);
            var tryTitle = tryTitleBase;

            Flier flierFind = null;

            if (queryService != null && (flierFind = queryService.FindByFriendlyId<Flier>(tryTitle)) != null
                && flierFind.Id != targetFlier.Id)
            {
                if (targetFlier.Location.HasAddressParts())
                {
                    var locInfo = targetFlier.Location.Locality;
                    if (string.IsNullOrWhiteSpace(locInfo))
                        locInfo = targetFlier.Location.PostCode;
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
            while ((flierFind = queryService.FindByFriendlyId<Flier>(tryTitle)) != null
                && flierFind.Id != targetFlier.Id)
            {
                tryTitle = (counter++) + "_" + tryTitleBase;
            }
            return tryTitle;
        }
    }
}
