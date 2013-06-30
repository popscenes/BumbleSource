using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ninject;
using Ninject.Syntax;
using Website.Infrastructure.Publish;
using Website.Infrastructure.Query;

namespace Website.Common.Model.Query
{
    public static class ToViewModelQueryChannelExtensions
    {

        public static ViewModelType ToViewModel<ViewModelType, SourceType>(this QueryChannelInterface queryChannel, SourceType source, ViewModelType current = null) 
            where ViewModelType : class
        {
            var root = queryChannel.Query(new GetResolutionRootQuery(), (IResolutionRoot) null);
            var mapper = root.Get<ViewModelMapperInterface<ViewModelType, SourceType>>();

            return mapper.ToViewModel(current, source);
        }

        public static List<ViewModelType> ToViewModel<ViewModelType, SourceType>(this QueryChannelInterface queryChannel, IEnumerable<SourceType> source)
    where ViewModelType : class
        {
            if (source == null) return null;
            var root = queryChannel.Query(new GetResolutionRootQuery(), (IResolutionRoot)null);
            var mapper = root.Get<ViewModelMapperInterface<ViewModelType, SourceType>>();
            return source.Select(s => mapper.ToViewModel(null, s)).ToList();
        }
    }

    public class GetResolutionRootQuery : QueryInterface
    {       
    }

    public class GetResolutionRootQueryHandler :
    QueryHandlerInterface<GetResolutionRootQuery, IResolutionRoot>
    {
        private readonly IResolutionRoot _resolutionRoot;

        public GetResolutionRootQueryHandler(IResolutionRoot resolutionRoot)
        {
            _resolutionRoot = resolutionRoot;
        }

        public IResolutionRoot Query(GetResolutionRootQuery argument)
        {
            return _resolutionRoot;
        }
    }

}
