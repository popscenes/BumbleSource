using System;
using PostaFlya.Domain.Command;
using WebSite.Infrastructure.Command;

namespace PostaFlya.Domain.Browser.Command
{
    public class SetDistanceCommand: DomainCommandBase
    {
        public string BrowserId {get; set; }
        public int Distance {get; set; }
    }
}