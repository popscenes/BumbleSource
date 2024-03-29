﻿using System;
using System.Collections.Generic;
using System.Linq;
using PostaFlya.Domain.Flier;
using PostaFlya.Domain.Flier.Query;
using Website.Infrastructure.Query;

namespace PostaFlya.DataRepository.Search.Query
{
    public class FindFlyersByBoardQueryHandler : QueryHandlerInterface<FindFlyersByBoardQuery, List<Flier>>
    {

        private readonly FlierSearchServiceInterface _searchService;
        private readonly GenericQueryServiceInterface _queryService;

        public FindFlyersByBoardQueryHandler(FlierSearchServiceInterface searchService, GenericQueryServiceInterface queryService)
        {
            _searchService = searchService;
            _queryService = queryService;
        }


        public List<Flier> Query(FindFlyersByBoardQuery argument)
        {
            var startDate = argument.Start.DateTime.Date;
            var endDate = argument.End.DateTime.Date;
            if (argument.Start.Offset == TimeSpan.Zero)
            {
                startDate = argument.Start.UtcDateTime.Date;
                endDate = argument.End.UtcDateTime.Date;
            }
            var ids = _searchService.FindFlyersByBoard(argument.BoardId, startDate,
                                                       endDate);

            return _queryService.FindByIds<Flier>(ids).ToList();
        }
    }
}