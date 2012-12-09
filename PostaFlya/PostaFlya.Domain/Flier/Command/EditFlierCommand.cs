using System;
using Website.Domain.Payment;
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
        public bool AllowUserContact { get; set; }
        public bool AttachTearOffs { get; set; }

        public static HashSet<EntityFeatureCharge> GetPaymentFeatures(EditFlierCommand editCommand, string browserId)
        {
            var featureList = new HashSet<EntityFeatureCharge>();
//            if (editCommand.AttachTearOffs)
//            {
//                featureList.Add(new SimpleEntityFeatureCharge() { FeatureType = FeatureType.PostAreaCharge, Cost = 80, BrowserId = browserId });
//            }
//
//            if (editCommand.AllowUserContact)
//            {
//                featureList.Add(new SimpleEntityFeatureCharge() { FeatureType = FeatureType.UserContact, Cost = 500, BrowserId = browserId });
//            }

            return featureList;
        }
    }
}