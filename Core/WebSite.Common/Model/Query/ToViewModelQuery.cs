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
//        public static ViewModelType ToViewModel<ViewModelType>(this QueryChannelInterface queryChannel, object source)
//        {
//            return queryChannel.Query(new ToViewModelQuery<ViewModelType>() {Source = source}, default(ViewModelType));
//        }

        public static ViewModelType ToViewModel<ViewModelType>(this QueryChannelInterface queryChannel, object source) where ViewModelType : class
        {
            return queryChannel.Query(new ToViewModelDynamicQuery() { Source = source, TypeOfViewModel = typeof(ViewModelType)}, default(object))
                as ViewModelType;
        }
    }


    internal class ToViewModelQuery<ViewModelType, SourceType> : QueryInterface
    {
        public SourceType Source { get; set; }
        public ViewModelType ViewModel { get; set; }
    }

    internal class ToViewModelQueryHandler<ViewModelType, SourceType> :
        QueryHandlerInterface<ToViewModelQuery<ViewModelType, SourceType>, ViewModelType>
    {
        private readonly IResolutionRoot _resolutionRoot;

        public ToViewModelQueryHandler(IResolutionRoot resolutionRoot)
        {
            _resolutionRoot = resolutionRoot;
        }

        public ViewModelType Query(ToViewModelQuery<ViewModelType, SourceType> argument)
        {
            var mapper = _resolutionRoot.Get<ViewModelMapperInterface<ViewModelType, SourceType>>();

            return mapper.ToViewModel(argument.ViewModel, argument.Source);
        }
    }

    internal class ToViewModelDynamicQuery : QueryInterface
    {
        public object Source { get; set; }
        public object ViewModel { get; set; }
        public Type TypeOfViewModel { get; set; }
    }

    internal class ToViewModelDynamicQueryHandler : QueryHandlerInterface<ToViewModelDynamicQuery, object>
    {
        private readonly IResolutionRoot _resolutionRoot;
        private readonly Type _genericViewModelMapperTyp = typeof(ViewModelMapperInterface<,>);

        public ToViewModelDynamicQueryHandler(IResolutionRoot resolutionRoot)
        {
            _resolutionRoot = resolutionRoot;
        }

        public object Query(ToViewModelDynamicQuery argument)
        {
            var mapperTyp = _genericViewModelMapperTyp.MakeGenericType(argument.TypeOfViewModel, argument.Source.GetType());
            dynamic mapper = _resolutionRoot.Get(mapperTyp);
            dynamic source = argument.Source;
            dynamic vm = argument.ViewModel;


            return mapper.ToViewModel(vm, source);
        }
    }
}
