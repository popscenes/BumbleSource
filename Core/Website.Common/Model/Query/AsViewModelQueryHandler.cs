using System.Collections.Generic;
using Website.Infrastructure.Domain;
using Website.Infrastructure.Query;

namespace Website.Common.Model.Query
{

    public class AsViewModelsQueryHandler<QueryType, EntityType, ViewModelType> :
    QueryHandlerInterface<QueryType, List<ViewModelType>>
        where EntityType : class, EntityIdInterface, new()
        where ViewModelType : class, IsModelInterface, new()
        where QueryType : class, QueryInterface<EntityType>, new()
    {
        private readonly QueryChannelInterface _queryChannel;


        public AsViewModelsQueryHandler(QueryChannelInterface queryChannel)
        {
            _queryChannel = queryChannel;
        }

        public List<ViewModelType> Query(QueryType argument)
        {
            var ret = _queryChannel.Query(argument, (List<EntityType>)null);
            return ret == null ? null : _queryChannel.ToViewModel<ViewModelType, EntityType>(ret);
        }
    }

    public class AsViewModelQueryHandler<QueryType, EntityType, ViewModelType> :
        QueryHandlerInterface<QueryType, ViewModelType>
        where EntityType : class, EntityIdInterface, new()
        where ViewModelType : class, IsModelInterface, new()
        where QueryType : class, QueryInterface<EntityType>, new()
    {
        private readonly QueryChannelInterface _queryChannel;


        public AsViewModelQueryHandler(QueryChannelInterface queryChannel)
        {
            _queryChannel = queryChannel;
        }


        public ViewModelType Query(QueryType argument)
        {
            var ret = _queryChannel.Query(argument, (EntityType)null);
            return ret == null ? null : _queryChannel.ToViewModel<ViewModelType, EntityType>(ret);
        }
    }

}