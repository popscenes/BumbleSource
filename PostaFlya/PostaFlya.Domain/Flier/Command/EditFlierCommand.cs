using System;
using PostaFlya.Domain.Venue;
using Website.Domain.Contact;
using Website.Domain.Payment;
using Website.Infrastructure.Command;
using System.Collections.Generic;
using Website.Domain.Tag;

namespace PostaFlya.Domain.Flier.Command
{
    public class EditFlierCommand : DefaultCommandBase
    {
        public Guid? Image { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
        public string Id { get; set; }
        public string BrowserId { get; set; }
        public List<DateTimeOffset> EventDates { get; set; }
        public Tags Tags { get; set; }
        public List<FlierImage> ImageList { get; set; }
        public HashSet<string> BoardSet { get; set; }
        public bool AllowUserContact { get; set; }
        public int ExtendPostRadius { get; set; }
        public bool EnableAnalytics { get; set; }
        public VenueInformation Venue { get; set; }
        public List<UserLink> UserLinks{ get; set; }
    }
}