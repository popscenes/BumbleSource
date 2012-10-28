using System;
using System.Text;

namespace Website.Domain.Browser.Query
{
    public static class BrowserQueryServiceUtil
    {
        public static string FindFreeHandle(this BrowserQueryServiceInterface queryService, string handle, string id)
        {
            var stringBuilder = new StringBuilder();
            foreach (var c in handle)
            {
                if (Char.IsLetterOrDigit(c) || c == '_')
                    stringBuilder.Append(Char.ToLower(c));
            }
            var tryHandle = stringBuilder.ToString();
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
