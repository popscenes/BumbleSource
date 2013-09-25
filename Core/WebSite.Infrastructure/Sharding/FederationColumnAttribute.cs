using System;
using System.ComponentModel;

namespace Website.Infrastructure.Sharding
{
    [AttributeUsage(AttributeTargets.Property)]
    public class FederationColumnAttribute : Attribute {
        public FederationColumnAttribute()
        {
            IsReferenceTable = false;
        }

        public string FederationName { get; set; }
        public string DistributionName { get; set; }
        [DefaultValue(false)]
        public bool IsReferenceTable { get; set; }
    }
}