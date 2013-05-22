using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace Website.Common.MediaFormatters
{
    public static class RegisterMediaFormatters
    {
        public static void For(HttpConfiguration config)
        {
            config.Formatters.Insert(0, new JsonpMediaTypeFormatter());
        }
    }
}
