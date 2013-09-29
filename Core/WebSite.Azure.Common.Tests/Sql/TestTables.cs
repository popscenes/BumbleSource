using System;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Types;
using NUnit.Framework;
using Website.Azure.Common.Sql;
using Website.Infrastructure.Domain;
using Website.Infrastructure.Sharding;

namespace Website.Azure.Common.Tests.Sql
{
    internal class SqlInitializerTestTable
    {
        public Guid Id { get; set; }

        public string Stringcol { get; set; }

        public int Intcol { get; set; }

        public double Doublecol { get; set; }

        public DateTimeOffset Datecol { get; set; }

        public DateTime Datetimecol { get; set; }

        public SqlXml XmlCol { get; set; }

        public long LongCol { get; set; }

        public SqlGeography Geography { get; set; }
    }


    internal class SqlInitializerTestFedRefernceTableWithIndex
    {
        public string Stringcol { get; set; }

        public long LongCol { get; set; }

        [FederationProperty(FederationName = "TestFederation", DistributionName = "long_col", IsReferenceTable = true)]        
        public long FedRefCol { get; set; }

    }

    internal class SqlInitializerTestIndexTable
    {
        public string Stringcol { get; set; }

        public long LongCol { get; set; }

    }

    internal class SqlInitializerTestFederatedTable
    {
        public Guid Id { get; set; }

        public string Stringcol { get; set; }

        public int Intcol { get; set; }

        public double Doublecol { get; set; }

        public DateTimeOffset Datecol { get; set; }

        public SqlXml XmlCol { get; set; }

        [FederationProperty(FederationName = "TestFederation", DistributionName = "long_col")]
        public long LongCol { get; set; }


        public SqlGeography Geography { get; set; }
    }

    internal class SqlInitializerTestAnotherFederatedTable
    {
        public Guid Id { get; set; }

        public string Stringcol { get; set; }

        public int Intcol { get; set; }

        public double Doublecol { get; set; }

        public DateTimeOffset Datecol { get; set; }

        public SqlXml XmlCol { get; set; }

        [FederationProperty(FederationName = "TestFederationAnother", DistributionName = "fed_col")]
        public Guid FedCol { get; set; }

        public SqlGeography Geography { get; set; }
    }

    internal class SqlInitializerTestAnotherFederatedTableOnString
    {
        public Guid Id { get; set; }

        [FederationProperty(FederationName = "TestFederationAnotherString", DistributionName = "fed_col")]
        public string Stringcol { get; set; }

        public int Intcol { get; set; }

        public double Doublecol { get; set; }

        public DateTimeOffset Datecol { get; set; }

        public SqlXml XmlCol { get; set; }

        
        public Guid FedCol { get; set; }

        public SqlGeography Geography { get; set; }
    }
}
