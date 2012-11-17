using System;
using Website.Infrastructure.Command;
using System.Collections.Generic;
using Website.Domain.Location;
using Website.Domain.Tag;

namespace PostaFlya.Domain.Flier.Command
{
    public class EditFlierCommand : DefaultCommandBase
    {
        public Guid? Image { get; set; }
        public Location Location { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
        public string Id { get; set; }
        public string BrowserId { get; set; }
        public DateTime EffectiveDate { get; set; }
        public Tags Tags { get; set; }
        public List<FlierImage> ImageList { get; set; }
        public HashSet<string> BoardSet { get; set; }
        public FlierFeaturesCommand FlierFeaturesCommand { get; set; }
    }
}