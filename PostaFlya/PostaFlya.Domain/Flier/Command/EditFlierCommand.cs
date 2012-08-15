using System;
using PostaFlya.Domain.Command;
using PostaFlya.Domain.Tag;
using WebSite.Infrastructure.Command;
using System.Collections.Generic;

namespace PostaFlya.Domain.Flier.Command
{
    public class EditFlierCommand : DomainCommandBase
    {
        public Guid? Image { get; set; }
        public Location.Location Location { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
        public string Id { get; set; }
        public string BrowserId { get; set; }
        public DateTime EffectiveDate { get; set; }
        public Tags Tags { get; set; }
        public List<FlierImage> ImageList { get; set; }

    }
}