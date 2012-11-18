using System;
using System.Text;
using Website.Infrastructure.Query;
using Website.Infrastructure.Util.Extension;

namespace Website.Domain.Browser.Query
{
    public static class BrowserQueryServiceUtil
    {
        public static string FindFreeHandleForBrowser(this GenericQueryServiceInterface queryService, string handle, string id)
        {
            var tryHandle = handle.ToLowerUnderScore();
            var tryHandleBase = tryHandle;

            int counter = 0;
            BrowserInterface brows = null;
            while ((brows = queryService.FindByFriendlyId<Browser>(tryHandle)) != null && brows.Id != id)
            {
                if (counter == 0 && Char.IsDigit(tryHandle[tryHandleBase.Length - 1]))
                {
                    counter = int.Parse("" + tryHandleBase[tryHandle.Length - 1]) + 1;
                    tryHandleBase = tryHandle.Substring(0, tryHandle.Length - 1);
                }

                tryHandle = tryHandleBase + counter++;
            }
            return tryHandle;
        }
    }
}
