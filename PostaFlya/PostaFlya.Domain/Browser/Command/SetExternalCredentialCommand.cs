using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PostaFlya.Domain.Command;
using WebSite.Infrastructure.Authentication;

namespace PostaFlya.Domain.Browser.Command
{
    public class SetExternalCredentialCommand : DomainCommandBase
    {
        public String BrowserId { get; set; }
        public IdentityProviderCredential Credential { get; set; }
    }
}
