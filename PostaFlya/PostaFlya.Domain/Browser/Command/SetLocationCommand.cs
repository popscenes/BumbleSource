using System;
using PostaFlya.Domain.Command;
using WebSite.Infrastructure.Command;

namespace PostaFlya.Domain.Browser.Command
{
    public class SetLocationCommand : DomainCommandBase
    {
        public string BrowserId { get; set; }
        public Location.Location Location { get; set; }
    }
}