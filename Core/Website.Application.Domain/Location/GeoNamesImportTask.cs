using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using GeoNamesImporter;
using NLog;
using Website.Domain.Location;
using Website.Domain.Location.Command;
using Website.Infrastructure.Binding;
using Website.Infrastructure.Configuration;
using Website.Infrastructure.Messaging;
using Website.Infrastructure.Task;
using Website.Infrastructure.Util.Extension;

namespace Website.Application.Domain.Location
{
    public class GeoNamesImportTask : ConsoleTask
    {
        private readonly MessageBusInterface _messageBus;
        private readonly ConfigurationServiceInterface _config;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public GeoNamesImportTask([WorkerCommandBus]MessageBusInterface messageBus
            , ConfigurationServiceInterface config)
        {
            _messageBus = messageBus;
            _config = config;
        }

        private List<Suburb> RunFromGeoNames(string[] args)
        {
            var reader = new GeoNamesReader();

            var suburbs = reader.ReadGeoNameTable(args[0], GeoNamesReader.DefaultPlaceIncludeCodes);
            var states = reader.ReadAdminCodesTable(args[1], GeoNamesReader.DefaultCountryIncludeCodes);
            var countries = reader.ReadCountryTable(args[2]);
            var postcodes = reader.ReadPostalCodeData(args[3]);
            var regions = reader.ReadAdmin2CodesTable(args[4]);

            var data = reader.MashTables(suburbs, countries, states, regions, postcodes)
                .Where(suburbData => !string.IsNullOrWhiteSpace(suburbData.regionname));//no state ignore

            var stateabbr = postcodes.Select(p => new { stateabb = p.admincode1, state = p.adminname1 })
                     .ToLookup(arg => arg.state)
                     .ToDictionary(grouping => grouping.Key.ToLower(), grouping => grouping.First().stateabb);

            var ausPostreader = new AusPostReader();
            var aus = ausPostreader.ReadPostalCodeData(args[5])
                .ToLookup(s => s.Locality.ToLower() + "|" + s.State.ToLower() + "|" + s.CountryCode.ToLower());

            var missingPostCodes = data.Where(suburbData => string.IsNullOrWhiteSpace(suburbData.postcode))
                .ToList();

            missingPostCodes.ForEach(s =>
            {
                var postmatches = aus[s.suburbname.ToLower() + "|" + stateabbr[s.regionname.ToLower()] + "|" + s.countrycode.ToLower()];
                if (postmatches.Any())
                {
                    s.postcode = postmatches.First().Pcode;
                    s.regionabbr = postmatches.First().State;
                }
            });

            return ToSubs(data);
        }
        protected override void Run(string[] args)
        {

            var ret = RunFromGeoNames(args);

            //var te = ret.SingleOrDefault(suburb => suburb.Locality.ToLower().Equals("petersham"));

            var test = ret.GroupBy(suburb => suburb.Id).Where(suburbs => suburbs.Count() > 1).ToList();
            if (test.Any())
            {
                Logger.Error("duplicate suburbs cancelling task");
                foreach (var subg in test)
                {
                    Logger.Error(subg.First().Locality + " " + subg.First().Region);
                }
                return;
            }

            var skip = _config.GetSetting<bool>("skipupload", true);

            var watch = new Stopwatch();
            watch.Start();
            int cnt = 0;
            foreach (var sub in ret)
            {
                if (!skip)
                _messageBus.Send(new AddOrUpdateSuburbCommand()
                    {
                        Update = sub
                    });

                cnt++;

                var time = watch.ElapsedMilliseconds;
                var av = (time > 0) ? cnt / time : 0;
                var left = ((ret.Count - cnt)*av)/1000;

                Logger.Info("Sent {0} of {1} suburb {2} esttime remaining {3} sec"
                    , cnt, ret.Count, sub.Locality + " " + sub.RegionCode, left);
            }


        }

        private static List<Suburb> ToSubs(IEnumerable<GeoNamesReader.SuburbData> data)
        {
            var subs = data.Select(s => new Suburb()
            {
                CountryCode = s.countrycode,
                CountryName = s.countryname,
                Locality = s.suburbname,
                LocalityExternalId = s.suburbgeonamesid.ToString(),
                ExternalSrc = "GeoNames",
                PostCode = s.postcode,
                Region = s.regionname,
                RegionCode = s.regionabbr,
                RegionExternalId = s.regiongeonameid.ToString(),
                Latitude = s.latitude,
                Longitude = s.longitude

            }).         
            ToList();

            subs.ForEach(suburb => suburb.Id = suburb.FriendlyId = suburb.GetGeneratedId());

            return subs;
        }

    }
}