using System;
using System.Linq;
using GeoNamesImporter;
using Website.Infrastructure.Configuration;
using Website.Infrastructure.Messaging;
using Website.Infrastructure.Task;

namespace Website.Application.Domain.Location
{
    public class GeoNamesImportTask : ConsoleTask
    {
        private MessageBusInterface _messageBus;
        private readonly ConfigurationServiceInterface _config;


        public GeoNamesImportTask(MessageBusInterface messageBus
            , ConfigurationServiceInterface config)
        {
            _messageBus = messageBus;
            _config = config;
        }

        protected override void Run(string[] args)
        {
            var test = _config.GetSetting("geonamesdosomething");

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