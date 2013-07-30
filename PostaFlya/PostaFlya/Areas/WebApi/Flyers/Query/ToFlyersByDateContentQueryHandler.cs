using System;
using System.Collections.Generic;
using System.Linq;
using PostaFlya.Areas.WebApi.Flyers.Model;
using PostaFlya.Domain.Flier.Query;
using Website.Infrastructure.Query;

namespace PostaFlya.Areas.WebApi.Flyers.Query
{
    public class ToFlyersByDateContentQueryHandler
        : QueryHandlerInterface<FindFlyersByDateAndLocationQuery, FlyersByDateContent>,
        QueryHandlerInterface<FindFlyersByBoardQuery, FlyersByDateContent>
    {
        private readonly QueryChannelInterface _queryChannel;

        public ToFlyersByDateContentQueryHandler(QueryChannelInterface queryChannel)
        {
            _queryChannel = queryChannel;
        }

        public FlyersByDateContent Query(FindFlyersByDateAndLocationQuery argument)
        {
            var flyers = _queryChannel.Query(argument, new List<FlyerSummaryModel>());

            return ToFlyersByDateContent(flyers, argument.Start.Date, argument.End.Date);

        }

        public FlyersByDateContent Query(FindFlyersByBoardQuery argument)
        {
            var flyers = _queryChannel.Query(argument, new List<FlyerSummaryModel>());

            return ToFlyersByDateContent(flyers, argument.Start.Date, argument.End.Date);

        }

        private FlyersByDateContent ToFlyersByDateContent(List<FlyerSummaryModel> flyers,
            DateTime startDate, DateTime endDate)
        {
            var bydate = from f in flyers
                         from fd in f.EventDates
                         where fd.Date >= startDate && fd.Date < endDate
                         select new { f.Id, fd.Date }
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