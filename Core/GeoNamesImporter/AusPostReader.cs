using System.Collections.Generic;
using System.IO;
using System.Linq;
using CsvHelper;

namespace GeoNamesImporter
{
    public class AusPostReader
    {
        public class SuburbData
        {
            public SuburbData()
            {
                CountryCode = "AU";
            }

            public string Pcode { get; set; }
            public string Locality { get; set; }
            public string State { get; set; }	
            public string Comments { get; set; }
            public string DeliveryOffice { get; set; }	
            public string PreSortIndicator { get; set; }
            public string ParcelZone { get; set; }
            public string BSPNumber { get; set; }
            public string BSPname { get; set; }
            public string Category { get; set; }
            public string CountryCode { get; set; }

        }

        public IEnumerable<SuburbData> ReadPostalCodeData(string fileName)
        {
            using (TextReader reader = File.OpenText(fileName))
            {
                var doc = CsvReader(reader);

                var res = doc.GetRecords<SuburbData>();
                return res.Where(s => s.Category.Equals("Delivery Area")).ToList();
            }
        }

        private static CsvReader CsvReader(TextReader reader)
        {
            var doc = new CsvReader(reader);
            doc.Configuration.HasHeaderRecord = true;
            doc.Configuration.AllowComments = true;
            doc.Configuration.AutoMap(typeof (SuburbData));
            doc.Configuration.TrimHeaders = true;
            doc.Configuration.WillThrowOnMissingField = false;
            return doc;
        }
    }
}