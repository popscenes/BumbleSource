using System.Linq;
using Ninject;
using Ninject.Syntax;

namespace Website.Infrastructure.Query
{
    public interface QueryChannelInterface
    {
        ReturnType Query<ReturnType, QueryType>(QueryType query, ReturnType defaultRet = default(ReturnType))
            where QueryType : QueryInterface;
    }

    public class DefaultQueryChannel : QueryChannelInterface
    {
        private readonly IResolutionRoot _resolutionRoot;

        public DefaultQueryChannel(IResolutionRoot resolutionRoot)
        {
            _resolutionRoot = resolutionRoot;
        }

        public ReturnType Query<ReturnType, QueryType>(QueryType query, ReturnType defaultRet = default(ReturnType)) 
            where QueryType : QueryInterface
        {
            //var ret = _resolutionRoot.Get<QueryHandlerInterface<QueryType, ReturnType>>();
            var ret = _resolutionRoot.GetAll<QueryHandlerInterface<QueryType, ReturnType>>();
           // return ret == null ? defaultRet : ret.Query(query);
            return !ret.Any() ? defaultRet : ret.First().Query(query);
        }
    }
}