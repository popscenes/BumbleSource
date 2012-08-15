using System;
using PostaFlya.Domain.Behaviour;
using PostaFlya.Domain.Command;
using PostaFlya.Domain.Tag;
using WebSite.Infrastructure.Command;
using System.Collections.Generic;

namespace PostaFlya.Domain.Flier.Command
{
    public class CreateFlierCommand : DomainCommandBase
    {
        public Guid? Image { get; set; }
        public string BrowserId { get; set; }
        public Tags Tags { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public Location.Location Location { get; set; }
        public FlierBehaviour FlierBehaviour { get; set; }
        public Tags WebSiteTags { get;set;}
        public DateTime EffectiveDate { get; set; }
        public List<FlierImage> ImageList { get; set; }
    }
}