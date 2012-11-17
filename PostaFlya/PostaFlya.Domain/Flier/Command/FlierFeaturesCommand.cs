using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PostaFlya.Domain.Flier.Command
{
    public class FlierFeaturesCommand
    {
        public bool AttachTearOffs { get; set; }
        public bool AllowUserContact { get; set; }

        public static HashSet<EntityFeatureInterface> GetPaymentFeatures(FlierFeaturesCommand command, string browserId)
        {
            var featureList = new HashSet<EntityFeatureInterface>();
            if (command.AttachTearOffs)
            {
                featureList.Add(new SimpleEntityFeature() { FeatureType = FeatureType.TearOff, Cost = 2.00, BrowserId = browserId });
            }

            if (command.AllowUserContact)
            {
                featureList.Add(new SimpleEntityFeature() { FeatureType = FeatureType.UserContact, Cost = 5.00, BrowserId = browserId });
            }

            return featureList;

        }
    }
}
