using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeoNamesImporter;
using Website.Infrastructure.Task;

namespace TaskConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var runner = new TaskRunner();
            runner.LoadModulesAndTasks();
            runner.RunTask(args);

            var reader = new GeoNamesReader();

            var suburbs = reader.ReadGeoNameTable(args[0], GeoNamesReader.DefaultPlaceIncludeCodes);
            var states = reader.ReadAdminCodesTable(args[1], GeoNamesReader.DefaultCountryIncludeCodes);
            var countries = reader.ReadCountryTable(args[2]);
            var postcodes = reader.ReadPostalCodeData(args[3]);
            var regions = reader.ReadAdmin2CodesTable(args[4]);

            var data = reader.MashTables(suburbs, countries, states, regions, postcodes);
            var dups = data.ToLookup(s => s.suburbname + "," + s.statename).Where(datas => datas.Count() > 1).ToList();
            var be = data.Where(s => s.suburbname.ToLower().Contains("brunswick"));

        }
    }
}
