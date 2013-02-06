using System;
using PostaFlya.Domain.Behaviour;
using Website.Domain.Contact;
using Website.Infrastructure.Command;
using System.Collections.Generic;
using Website.Domain.Location;
using Website.Domain.Tag;

namespace PostaFlya.Domain.Flier.Command
{
    public class CreateFlierCommand : DefaultCommandBase
    {
        public Guid? Image { get; set; }
        public string BrowserId { get; set; }
        public Tags Tags { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public Location Location { get; set; }
        public FlierBehaviour FlierBehaviour { get; set; }
        public Tags WebSiteTags { get;set;}
        public DateTime EffectiveDate { get; set; }
        public List<FlierImage> ImageList { get; set; }
        public string ExternalSource { get; set; }
        public string ExternalId { get; set; }
        public HashSet<string> BoardSet { get; set; }
        public bool AllowUserContact { get; set; }
        public bool AttachTearOffs { get; set; }
        public int ExtendPostRadius { get; set; }
        public bool EnableAnalytics { get; set; }
        public ContactDetails ContactDetails { get; set; }
    }
}