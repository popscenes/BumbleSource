using System;
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
        public static ViewModelType ToViewModel<ViewModelType>(this QueryChannelInterface queryChannel, object source)
        {
            return queryChannel.Query(new ToViewModelQuery<ViewModelType>() {Source = source}, default(ViewModelType));
        }
    }

    public class ToViewModelQuery<ViewModelType> : QueryInterface
    {
        public object Source { get; set; }
        public ViewModelType ViewModel { get; set; }
    }


    internal class ToViewModelQueryHandler<ViewModelType> :
        QueryHandlerInterface<ToViewModelQuery<ViewModelType>, ViewModelType>
    {
        private readonly IResolutionRoot _resolutionRoot;
        private readonly Type _genericViewModelMapperTyp = typeof(ViewModelMapperInterface<,>);

        public ToViewModelQueryHandler(IResolutionRoot resolutionRoot)
        {
            _resolutionRoot = resolutionRoot;
        }

        public ViewModelType Query(ToViewModelQuery<ViewModelType> argument)
        {
            var mapperTyp = _genericViewModelMapperTyp.MakeGenericType(typeof(ViewModelType), argument.Source.GetType());
            dynamic mapper = _resolutionRoot.Get(mapperTyp);

            return mapper.ToViewModel(argument.ViewModel, argument.Source);
        }
    }
}
