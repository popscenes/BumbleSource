using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebSite.Azure.Common.Sql
{
    public class PrimaryKey : Attribute { }
    public class NotNullable : Attribute { }
    public class SpatialIndex : Attribute { }
    public class FederationCol : Attribute {
        public string FederationName { get; set; }
        public string DistributionName { get; set; }
    }
}
