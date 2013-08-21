using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;

namespace GeoNamesImporter
{
//geonameid         : integer id of record in geonames database
//name              : name of geographical point (utf8) varchar(200)
//asciiname         : name of geographical point in plain ascii characters, varchar(200)
//alternatenames    : alternatenames, comma separated varchar(5000)
//latitude          : latitude in decimal degrees (wgs84)
//longitude         : longitude in decimal degrees (wgs84)
//featureclass     : see http://www.geonames.org/export/codes.html, char(1)
//featurecode      : see http://www.geonames.org/export/codes.html, varchar(10)
//countrycode      : ISO-3166 2-letter country code, 2 characters
//cc2               : alternate country codes, comma separated, ISO-3166 2-letter country code, 60 characters
//admin1code       : fipscode (subject to change to iso code), see exceptions below, see file admin1Codes.txt for display names of this code; varchar(20)
//admin2code       : code for the second administrative division, a county in the US, see file admin2Codes.txt; varchar(80) 
//admin3code       : code for third level administrative division, varchar(20)
//admin4code       : code for fourth level administrative division, varchar(20)
//population        : bigint (8 byte int) 
//elevation         : in meters, integer
//dem               : digital elevation model, srtm3 or gtopo30, average elevation of 3''x3'' (ca 90mx90m) or 30''x30'' (ca 900mx900m) area in meters, integer. srtm processed by cgiar/ciat.
//timezone          : the timezone id (see file timeZone.txt) varchar(40)
//modificationdate : date of last modification in yyyy-MM-dd format

    public class GeoNameTable
    {
        public string geonameid { get; set; }
        public string name { get; set; }
        public string asciiname { get; set; }
        public string alternatenames { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
        public string featureclass { get; set; }
        public string featurecode { get; set; }
        public string countrycode { get; set; }
        public string cc2 { get; set; }
        public string admin1code { get; set; }
        public string admin2code { get; set; }
        public string admin3code { get; set; }
        public string population { get; set; }
        public string elevation { get; set; }
        public string dem { get; set; }
        public string timezone { get; set; }
        public string modificationdate { get; set; }
    }

    public class GeoNameTableMap : CsvClassMap<GeoNameTable>
    {
        public override void CreateMap()
        {
            var index = 0;
            Map(table => table.geonameid).Index(index++);
            Map(table => table.name).Index(index++);
            Map(table => table.asciiname).Index(index++);
            Map(table => table.alternatenames).Index(index++);
            Map(table => table.latitude).Index(index++);
            Map(table => table.longitude).Index(index++);
            Map(table => table.featureclass).Index(index++);
            Map(table => table.featurecode).Index(index++);
            Map(table => table.countrycode).Index(index++);
            Map(table => table.cc2).Index(index++);
            Map(table => table.admin1code).Index(index++);
            Map(table => table.admin2code).Index(index++);
            Map(table => table.admin3code).Index(index++);
            Map(table => table.population).Index(index++);
            Map(table => table.elevation).Index(index++);
            Map(table => table.dem).Index(index++);
            Map(table => table.timezone).Index(index++);
            Map(table => table.modificationdate).Index(index++);
        }
    }

    //AU.04	Queensland	Queensland	2152274
    public class AdminCodesTable
    {
        public string countryadmin { get; set; }
        public string countrycode { get; set; }
        public string admin1code { get; set; }
        public string admin1NameUtf { get; set; }
        public string admin1NameAscii { get; set; }
        public string geonameid { get; set; }

    }

    public class AdminCodesTableMap : CsvClassMap<AdminCodesTable>
    {
        public override void CreateMap()
        {
            var index = 0;
            Map(table => table.countryadmin).Index(index++);
            Map(table => table.countrycode).ConvertUsing(row => row.GetField<string>(0).Split('.')[0]);
            Map(table => table.admin1code).ConvertUsing(row => row.GetField<string>(0).Split('.')[1]);
            Map(table => table.admin1NameUtf).Index(index++);
            Map(table => table.admin1NameAscii).Index(index++);
            Map(table => table.geonameid).Index(index++);

        }
    }

    //AU	AUS	036	AS	Australia	Canberra
    public class CountryTable
    {
        public string countrycode_iso2 { get; set; }
        public string iso3 { get; set; }
        public string isonum { get; set; }
        public string fips { get; set; }
        public string name { get; set; }
        public string capital { get; set; }
    }

    public class CountryTableMap : CsvClassMap<CountryTable>
    {
        public override void CreateMap()
        {
            var index = 0;
            Map(table => table.countrycode_iso2).Index(index++);
            Map(table => table.iso3).Index(index++);
            Map(table => table.isonum).Index(index++);
            Map(table => table.fips).Index(index++);
            Map(table => table.name).Index(index++);
            Map(table => table.capital).Index(index++);

        }
    }

    public class GeoNamesReader
    {
        public static readonly HashSet<string> DefaultPlaceIncludeCodes = new HashSet<string>()
            {
                "PPL",
                "ADMD",
            };
        public static readonly HashSet<string> DefaultCountryIncludeCodes = new HashSet<string>()
            {
                "AU",
            };

        public GeoNamesReader()
        {

        }

        public IEnumerable<GeoNameTable> ReadGeoNameTable(string fileName
            , HashSet<string> codesForInclude = null
            , HashSet<string> countriesForInclude = null
            , Func<IGrouping<string, GeoNameTable>, GeoNameTable> matchselector = null )
        {

            matchselector = matchselector ?? GetPriority;
            using (TextReader reader = File.OpenText(fileName))
            {
                var doc = CsvReader(reader);

                var res = doc.GetRecords<GeoNameTable>();
                return res
                    .Where(t => countriesForInclude == null || countriesForInclude.Contains(t.countrycode))
                    .Where(t => codesForInclude == null || codesForInclude.Contains(t.featurecode))
                    .ToLookup(table => table.name + table.admin1code + table.countrycode)
                    .Select(matchselector)
                    .ToList();

            }
        }

        private GeoNameTable GetPriority(IGrouping<string, GeoNameTable> tables)
        {
            if (tables.Count() == 1) 
                return tables.First();
            
            var ret = tables.FirstOrDefault(table => table.featurecode == "PPL");
            ret = ret ?? tables.FirstOrDefault(table => table.featurecode == "ADMD");
            return ret ?? tables.FirstOrDefault();
        }

        public IEnumerable<AdminCodesTable> ReadAdminCodesTable(string fileName, HashSet<string> countriesForInclude = null)
        {
            using (TextReader reader = File.OpenText(fileName))
            {
                var doc = CsvReader(reader);

                var res = doc.GetRecords<AdminCodesTable>();
                return res.Where(t => countriesForInclude == null || countriesForInclude.Contains(t.countrycode))
                    .ToList();

            }
        }

        public IEnumerable<CountryTable> ReadCountryTable(string fileName)
        {
            using (TextReader reader = File.OpenText(fileName))
            {
                var doc = CsvReader(reader);

                var res = doc.GetRecords<CountryTable>();
                return res.ToList();
            }
        }

        private static CsvReader CsvReader(TextReader reader)
        {
            var doc = new CsvReader(reader);
            doc.Configuration.RegisterClassMap<AdminCodesTableMap>();
            doc.Configuration.RegisterClassMap<GeoNameTableMap>();
            doc.Configuration.RegisterClassMap<CountryTableMap>();
            doc.Configuration.HasHeaderRecord = false;
            doc.Configuration.Delimiter = "\t";
            doc.Configuration.AllowComments = true;
            return doc;
        }
    }
}
