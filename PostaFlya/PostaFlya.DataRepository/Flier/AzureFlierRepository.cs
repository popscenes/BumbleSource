using System;
using System.Collections.Generic;
using System.Data.Services.Client;
using System.Linq;
using Ninject;
using WebSite.Azure.Common.TableStorage;
using PostaFlya.DataRepository.Internal;
using PostaFlya.DataRepository.Search.Services;
using PostaFlya.Domain.Behaviour;
using PostaFlya.Domain.Flier;
using PostaFlya.Domain.Flier.Command;
using PostaFlya.Domain.Flier.Query;
using WebSite.Infrastructure.Command;
using WebSite.Infrastructure.Query;
using Website.Domain.Location;
using Website.Domain.Tag;

namespace PostaFlya.DataRepository.Flier
{
    internal class AzureFlierRepository : JsonRepositoryWithBrowser
        , FlierRepositoryInterface
        , FlierQueryServiceInterface
    {
        private readonly FlierSearchServiceInterface _flierSearchService;


        #region Implementation of FlierQueryServiceInterface

        public AzureFlierRepository(TableContextInterface tableContext
            , TableNameAndPartitionProviderServiceInterface nameAndPartitionProviderService
            , FlierSearchServiceInterface flierSearchService)
            : base(tableContext, nameAndPartitionProviderService, flierSearchService)
        {
            _flierSearchService = flierSearchService;
        }

        public IList<string> FindFliersByLocationTagsAndDistance(Location location, Tags tags, int distance = 0, int take = 0, FlierSortOrder sortOrder = FlierSortOrder.CreatedDate, int skip = 0)
        {
            return _flierSearchService
                .FindFliersByLocationTagsAndDistance(location, tags, distance, take, sortOrder,
                                                                           skip);
       
        }


        #endregion


    }
}