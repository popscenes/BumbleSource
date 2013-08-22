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
        public string codesconcat { get; set; }
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
            Map(table => table.codesconcat).Index(index++);
            Map(table => table.countrycode).ConvertUsing(row => row.GetField<string>(0).Split('.')[0]);
            Map(table => table.admin1code).ConvertUsing(row => row.GetField<string>(0).Split('.')[1]);
            Map(table => table.admin1NameUtf).Index(index++);
            Map(table => table.admin1NameAscii).Index(index++);
            Map(table => table.geonameid).Index(index++);

        }
    }

    //AU.04	Queensland	Queensland	2152274
    public class Admin2CodesTable
    {
        public string codesconcat { get; set; }
        public string countrycode { get; set; }
        public string admin1code { get; set; }
        public string admin2code { get; set; }

        public string admin2NameUtf { get; set; }
        public string admin2NameAscii { get; set; }
        public string geonameid { get; set; }

    }

    public class Admin2CodesTableMap : CsvClassMap<Admin2CodesTable>
    {
        public override void CreateMap()
        {
            var index = 0;
            Map(table => table.codesconcat).Index(index++);
            Map(table => table.countrycode).ConvertUsing(row => row.GetField<string>(0).Split('.')[0]);
            Map(table => table.admin1code).ConvertUsing(row => row.GetField<string>(0).Split('.')[1]);
            Map(table => table.admin2code).ConvertUsing(row => row.GetField<string>(0).Split('.')[2]);
            Map(table => table.admin2NameUtf).Index(index++);
            Map(table => table.admin2NameAscii).Index(index++);
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

//    country code      : iso country code, 2 characters
//postal code       : varchar(20)
//place name        : varchar(180)
//admin name1       : 1. order subdivision (state) varchar(100)
//admin code1       : 1. order subdivision (state) varchar(20)
//admin name2       : 2. order subdivision (county/province) varchar(100)
//admin code2       : 2. order subdivision (county/province) varchar(20)
//admin name3       : 3. order subdivision (community) varchar(100)
//admin code3       : 3. order subdivision (community) varchar(20)
//latitude          : estimated latitude (wgs84)
//longitude         : estimated longitude (wgs84)
//accuracy          : accuracy of lat/lng from 1=estimated to 6=centroid


    public class PostCodeTable
    {
        public string countrycode { get; set; }
        public string postalcode { get; set; }
        public string placename { get; set; }
        public string adminname1 { get; set; }
        public string admincode1 { get; set; }
        public string adminname2 { get; set; }
        public string admincode2 { get; set; }
        public string adminname3 { get; set; }
        public string admincode3 { get; set; }
        public string latitude { get; set; }
        public string longitude { get; set; }
    }

    public class PostCodeTableMap : CsvClassMap<PostCodeTable>
    {
        public override void CreateMap()
        {
            var index = 0;
            Map(table => table.countrycode).Index(index++);
            Map(table => table.postalcode).Index(index++);
            Map(table => table.placename).Index(index++);
            Map(table => table.adminname1).Index(index++);
            Map(table => table.admincode1).Index(index++);
            Map(table => table.adminname2).Index(index++);
            Map(table => table.admincode2).Index(index++);
            Map(table => table.adminname3).Index(index++);
            Map(table => table.admincode3).Index(index++);
            Map(table => table.latitude).Index(index++);
            Map(table => table.longitude).Index(index++);

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
            , Func<IGrouping<string, GeoNameTable>, IEnumerable<GeoNameTable>> matchselector = null)
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
                    .SelectMany(matchselector)
                    .ToList();

            }
        }


        private IEnumerable<GeoNameTable> GetPriority(IGrouping<string, GeoNameTable> tables)
        {
            if (tables.Count() == 1) 
                return tables;

            return tables.Where(table => !string.IsNullOrWhiteSpace(table.admin2code))
                         .GroupBy(table => table.admin2code)
                         .Select(grouping =>
                                 grouping.FirstOrDefault(g => g.featurecode == "PPL") ??
                                 grouping.First());
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

        public IEnumerable<Admin2CodesTable> ReadAdmin2CodesTable(string fileName, HashSet<string> countriesForInclude = null)
        {
            using (TextReader reader = File.OpenText(fileName))
            {
                var doc = CsvReader(reader);

                var res = doc.GetRecords<Admin2CodesTable>();
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

        public IEnumerable<PostCodeTable> ReadPostalCodeData(string fileName)
        {
            using (TextReader reader = File.OpenText(fileName))
            {
                var doc = CsvReader(reader);

                var res = doc.GetRecords<PostCodeTable>();
                return res.ToList();
            }
        }
        
        public IEnumerable<SuburbData> MashTables(IEnumerable<GeoNameTable> suburbs
            , IEnumerable<CountryTable> countries, IEnumerable<AdminCodesTable> states
            , IEnumerable<Admin2CodesTable> regions, IEnumerable<PostCodeTable> postcodes)
        {
            var country = countries.ToDictionary(table => table.countrycode_iso2);
            var state = states.ToDictionary(table => table.codesconcat);
            var reg = regions.ToDictionary(table => table.codesconcat);

            var postcode =
                postcodes.ToLookup(table => table.placename + "." + table.countrycode + "." + table.adminname1);

            var list = suburbs.Select(s =>
                {
                    var ret = new SuburbData();

                    ret.concatcodes = s.countrycode + "." + s.admin1code + "." + s.admin2code;
                    ret.countrycode = s.countrycode;
                    ret.statecode = s.admin1code;
                    ret.regioncode = s.admin2code;
                    ret.suburbname = s.name;
                    ret.suburbgeonamesid = long.Parse(s.geonameid);

                    if (!string.IsNullOrWhiteSpace(s.admin1code))
                    {
                        var st = state[s.countrycode + "." + s.admin1code];
                        ret.statename = st.admin1NameUtf;
                        ret.stategeonameid = long.Parse(st.geonameid);
                    }
                    
                    if (!string.IsNullOrWhiteSpace(s.admin2code))
                    {
                        var rg = reg[ret.concatcodes];
                        ret.regionname = rg.admin2NameUtf;
                        ret.regionngeonameid = long.Parse(rg.geonameid); 
                    }

                    var ct = country[s.countrycode];
                    ret.countryname = ct.name;

                    ret.longitude = s.longitude;
                    ret.latitude = s.latitude;

                    var postmatches = postcode[ret.suburbname + "." + ret.countrycode + "." + ret.statename];
                    var code =
                        postmatches.FirstOrDefault(table =>
                            {
                                if (string.IsNullOrWhiteSpace(table.latitude)) return false;
                                if (Math.Abs(double.Parse(table.latitude) - s.latitude) > 0.5) return false;
                                if (Math.Abs(double.Parse(table.longitude) - s.longitude) > 0.5) return false;
                                return true;
                            });

                    ret.postcode = code != null ? code.postalcode : null;
                    ret.stateabbr = code != null ? code.admincode1 : null;
                    return ret;

                }).ToList();

            return list;
        }

        public class SuburbData
        {
            public long suburbgeonamesid { get; set; }
            public string suburbname { get; set; }
            
            public string statecode { get; set; }
            public string statename { get; set; }
            public long stategeonameid { get; set; }

            public string regioncode { get; set; }
            public string regionname { get; set; }
            public long regionngeonameid { get; set; }

            public string countrycode { get; set; }
            public string countryname { get; set; }

            public string concatcodes { get; set; }
            
            public string postcode { get; set; }
            public string stateabbr { get; set; }

            public double latitude { get; set; }
            public double longitude { get; set; }
        }

        private static CsvReader CsvReader(TextReader reader)
        {
            var doc = new CsvReader(reader);
            doc.Configuration.RegisterClassMap<AdminCodesTableMap>();
            doc.Configuration.RegisterClassMap<Admin2CodesTableMap>();
            doc.Configuration.RegisterClassMap<GeoNameTableMap>();
            doc.Configuration.RegisterClassMap<CountryTableMap>();
            doc.Configuration.RegisterClassMap<PostCodeTableMap>();
            doc.Configuration.HasHeaderRecord = false;
            doc.Configuration.Delimiter = "\t";
            doc.Configuration.AllowComments = true;
            return doc;
        }
    }
}
