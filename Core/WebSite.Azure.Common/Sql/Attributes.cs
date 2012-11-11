using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Website.Azure.Common.Sql
{
    public class PrimaryKey : Attribute { }
    public class NotNullable : Attribute { }
    public class SpatialIndex : Attribute { }
    //only have support/test for non-clustered, non-unique, single column
    //feel free to add support for the others
    public class SqlIndex : Attribute
    {
        public SqlIndex()
        {
            Clustered = false;
            Unique = false;
            Columns = null;
        }
        [DefaultValue(false)]
        public bool Clustered { get; set; }
        [DefaultValue(false)]
        public bool Unique { get; set; }
        [DefaultValue(null)]
        public string[] Columns { get; set; }
    }

    public class FederationCol : Attribute {
        public FederationCol()
        {
            IsReferenceTable = false;
        }

        public string FederationName { get; set; }
        public string DistributionName { get; set; }
        [DefaultValue(false)]
        public bool IsReferenceTable { get; set; }
    }
}
