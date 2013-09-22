using System;
using System.Collections.Generic;
using System.Linq;
using PostaFlya.Areas.WebApi.Flyers.Model;
using PostaFlya.Domain.Flier;
using PostaFlya.Domain.Flier.Query;
using PostaFlya.Models.Browser;
using Website.Common.Model.Query;
using Website.Domain.Browser.Query;
using Website.Domain.Claims;
using Website.Infrastructure.Query;

namespace PostaFlya.Areas.WebApi.Flyers.Query
{
    public class ToFlyersByDateContentQueryHandler
        : QueryHandlerInterface<FindFlyersByDateAndLocationQuery, FlyersByDateContent>,
        QueryHandlerInterface<FindFlyersByBoardQuery, FlyersByDateContent>,
        QueryHandlerInterface<GetByBrowserIdQuery<Claim>, FlyersByDateContent>
    {
        private readonly QueryChannelInterface _queryChannel;
        private readonly GenericQueryServiceInterface _queryService;

        public ToFlyersByDateContentQueryHandler(QueryChannelInterface queryChannel, GenericQueryServiceInterface queryService)
        {
            _queryChannel = queryChannel;
            _queryService = queryService;
        }

        public FlyersByDateContent Query(FindFlyersByDateAndLocationQuery argument)
        {
            var flyers = _queryChannel.Query(argument, new List<FlyerSummaryModel>());

            return ToFlyersByDateContent(flyers, argument.Start.Date, argument.End.Date);

        }

        public FlyersByDateContent Query(GetByBrowserIdQuery<Claim> argument)
        {
            var flyers = _queryChannel.Query(argument, new List<Claim>()).Select(l => _queryService.FindById<Flier>(l.AggregateId));
            var flyersViewModel = _queryChannel.ToViewModel<FlyerSummaryModel, Flier>(flyers);
            return ToFlyersByDateContent(flyersViewModel, DateTime.MinValue, DateTime.MaxValue);
        }

        public FlyersByDateContent Query(FindFlyersByBoardQuery argument)
        {
            var flyers = _queryChannel.Query(argument, new List<FlyerSummaryModel>());
            return ToFlyersByDateContent(flyers, argument.Start.Date, argument.End.Date);

        }

        private FlyersByDateContent ToFlyersByDateContent(List<FlyerSummaryModel> flyers,
            DateTime startDate, DateTime endDate)
        {
            var bydate = from f in flyers where f.Image.Extensions.Count > 0
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