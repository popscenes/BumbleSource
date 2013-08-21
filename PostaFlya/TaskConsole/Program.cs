using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeoNamesImporter;

namespace TaskConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var reader = new GeoNamesReader();

            var res = reader.ReadGeoNameTable(args[0], GeoNamesReader.DefaultPlaceIncludeCodes);
            var res3 = reader.ReadAdminCodesTable(args[1], GeoNamesReader.DefaultCountryIncludeCodes);
            var res4 = reader.ReadCountryTable(args[2]);
        }
    }
}
