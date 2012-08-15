using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebSite.Infrastructure.Authentication
{
    [Serializable]
    public class AccessToken
    {
        public String Token { get; set; }
        public String Permissions { get; set; }
        public DateTime Expires { get; set; }
    }
}
