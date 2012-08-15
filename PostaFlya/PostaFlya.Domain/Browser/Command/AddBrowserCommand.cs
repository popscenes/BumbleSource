using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PostaFlya.Domain.Command;
using WebSite.Infrastructure.Command;

namespace PostaFlya.Domain.Browser.Command
{
    public class AddBrowserCommand : DomainCommandBase
    {
        public Browser Browser { get; set; }
    }
}
