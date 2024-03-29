﻿using System.Linq;
using Website.Infrastructure.Domain;
using Website.Infrastructure.Query;

namespace Website.Mocks.Domain.DomainQuery
{
    public class TestFindByFriendlyIdQueryHandler<EntityType> : 
        QueryHandlerInterface<FindByFriendlyIdQuery<EntityType>, EntityType> where EntityType : class, AggregateRootInterface, new()
    {
        private readonly GenericQueryServiceInterface _queryService;


        public TestFindByFriendlyIdQueryHandler(GenericQueryServiceInterface queryService)
        {
            _queryService = queryService;
        }

        public EntityType Query(FindByFriendlyIdQuery<EntityType> argument)
        {
            var all = _queryService.GetAllIds<EntityType>().Select(_queryService.FindById<EntityType>);
            return all.FirstOrDefault(a => a.FriendlyId == argument.FriendlyId);    
        }
    }
}
