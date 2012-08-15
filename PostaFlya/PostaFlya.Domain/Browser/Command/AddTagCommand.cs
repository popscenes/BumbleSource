using System;
using PostaFlya.Domain.Command;
using PostaFlya.Domain.Tag;
using WebSite.Infrastructure.Command;

namespace PostaFlya.Domain.Browser.Command
{
    public class AddTagCommand : DomainCommandBase
    {
        public string BrowserId { get; set; }
        public Tags Tags { get; set; }
    }
}