using System;
using PostaFlya.Domain.Command;
using PostaFlya.Domain.Tag;
using WebSite.Infrastructure.Command;

namespace PostaFlya.Domain.Browser.Command
{
    public class SavedTagsDeleteCommand : DomainCommandBase
    {
        public Tags Tags { get; set; }
        public string BrowserId { get; set; }

    }
}