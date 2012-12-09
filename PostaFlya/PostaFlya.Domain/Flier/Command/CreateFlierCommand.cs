using System;
using PostaFlya.Domain.Behaviour;
using Website.Domain.Payment;
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

        public static HashSet<EntityFeatureChargeDecorator> GetPaymentFeatures(CreateFlierCommand createCommand, string browserId)
        {
            var featureList = new HashSet<EntityFeatureChargeDecorator>();
//            if (createCommand.AttachTearOffs)
//            {
//                featureList.Add(new SimpleEntityFeatureCharge() { FeatureType = FeatureType.PostAreaCharge, Cost = 80, BrowserId = browserId });
//            }
//
//            if (createCommand.AllowUserContact)
//            {
//                featureList.Add(new SimpleEntityFeatureCharge() { FeatureType = FeatureType.UserContact, Cost = 500, BrowserId = browserId });
//            }

            return featureList;
        }
    }
}