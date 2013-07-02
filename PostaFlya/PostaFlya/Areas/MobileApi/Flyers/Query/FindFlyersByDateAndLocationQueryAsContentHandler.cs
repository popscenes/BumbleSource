using System.Collections.Generic;
using System.Linq;
using System.Web;
using PostaFlya.Areas.MobileApi.Flyers.Model;
using PostaFlya.Domain.Flier.Query;
using Website.Infrastructure.Query;

namespace PostaFlya.Areas.MobileApi.Flyers.Query
{
    public class FindFlyersByDateAndLocationQueryAsContentHandler
        : QueryHandlerInterface<FindFlyersByDateAndLocationQuery, FlyersByDateContent>
    {
        private readonly QueryChannelInterface _queryChannel;

        public FindFlyersByDateAndLocationQueryAsContentHandler(QueryChannelInterface queryChannel)
        {
            _queryChannel = queryChannel;
        }

        public FlyersByDateContent Query(FindFlyersByDateAndLocationQuery argument)
        {
            var flyers = _queryChannel.Query(argument, new List<FlyerSummaryModel>());

            var bydate = from f in flyers
                         from fd in f.EventDates

                         select new {f.Id, fd.Date}
                         into s
                         group s by s.Date
                         into g
                         orderby g.Key
                         select new FlyersByDateContent.FlyersByDate()
                             {
                                 Date = g.Key,
                                 FlyerIds = g.Select(arg => arg.Id).ToList()
                             };

            return new FlyersByDateContent()
                {
                    Dates = bydate.ToList(),
                    Flyers = flyers.ToDictionary(model => model.Id)
                };


        }
    }
}