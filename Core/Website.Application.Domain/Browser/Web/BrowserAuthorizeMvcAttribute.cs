using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Mvc;
using Ninject;
using Ninject.Syntax;
using Website.Domain.Browser;
using Website.Infrastructure.Command;
using Website.Infrastructure.Util;

namespace Website.Application.Domain.Browser.Web
{
    public class BrowserAuthorizeMvcAttribute : AuthorizeAttribute
    {
        private string _roles;
        private string[] _rolesSplit = new string[0];

        public new string Roles
        {
            get
            {
                return _roles ?? String.Empty;
            }
            set
            {
                _roles = value;
                _rolesSplit = value.SplitStringTrimRemoveEmpty();
            }
        }

        [Inject]
        public IResolutionRoot ResolutionRoot { get; set; }

        protected BrowserInformationInterface BrowserInformation
        {
            get { return ResolutionRoot.Get<BrowserInformationInterface>(); }
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException("httpContext");
            }


            if (BrowserInformation.Browser.HasRole(Role.Admin))
                return true;

            if (
                (Roles.Length == 0 && !BrowserInformation.Browser.IsTemporary()) ||
                BrowserInformation.Browser.HasAnyRole(_rolesSplit)
                )
                return true;

            return false;

        }
    }
}
