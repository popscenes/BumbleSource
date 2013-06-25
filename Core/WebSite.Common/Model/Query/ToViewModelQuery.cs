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
//        public static ViewModelType ToViewModel<ViewModelType>(this QueryChannelInterface queryChannel, object source)
//        {
//            return queryChannel.Query(new ToViewModelQuery<ViewModelType>() {Source = source}, default(ViewModelType));
//        }

        public static ViewModelType ToViewModel<ViewModelType, SourceType>(this QueryChannelInterface queryChannel, SourceType source, ViewModelType current = null) 
            where ViewModelType : class
        {
            return queryChannel.Query(new ToViewModelDynamicQuery()
                {
                    TypeOfSource = typeof(SourceType),
                    Source = source, 
                    TypeOfViewModel = typeof(ViewModelType), ViewModel = current
                }, default(object))
                as ViewModelType;
        }

        public static List<ViewModelType> ToViewModel<ViewModelType, SourceType>(this QueryChannelInterface queryChannel, IEnumerable<SourceType> source)
    where ViewModelType : class
        {
            var ret =
                queryChannel.Query(
                    new ToViewModelListDynamicQuery()
                        {
                            TypeOfSource = typeof(SourceType),
                            Source = source.Cast<object>().ToList(), 
                            TypeOfViewModel = typeof (ViewModelType)
                        },
                    new List<object>());
            return ret.OfType<ViewModelType>().ToList();
        }
    }


    public class ToViewModelQuery<ViewModelType, SourceType> : QueryInterface
    {
        public SourceType Source { get; set; }
        public ViewModelType ViewModel { get; set; }
    }

    public class ToViewModelQueryHandler<ViewModelType, SourceType> :
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

    public class ToViewModelDynamicQuery : QueryInterface
    {
        public object Source { get; set; }
        public object ViewModel { get; set; }
        public Type TypeOfViewModel { get; set; }
        public Type TypeOfSource { get; set; }
    }

    public class ToViewModelListDynamicQuery : QueryInterface
    {
        public List<object> Source { get; set; }
        public List<object> ViewModel { get; set; }
        public Type TypeOfViewModel { get; set; }
        public Type TypeOfSource { get; set; }
    }

    public class ToViewModelListDynamicQueryHandler : QueryHandlerInterface<ToViewModelListDynamicQuery, List<object>>
    {
        private readonly IResolutionRoot _resolutionRoot;
        private readonly Type _genericViewModelMapperTyp = typeof(ViewModelMapperInterface<,>);

        public ToViewModelListDynamicQueryHandler(IResolutionRoot resolutionRoot)
        {
            _resolutionRoot = resolutionRoot;
        }

        public List<object> Query(ToViewModelListDynamicQuery argument)
        {
            var sourceFor = argument.Source.FirstOrDefault();
            if (sourceFor == null)
                return new List<object>();

            var mapperTyp = _genericViewModelMapperTyp.MakeGenericType(argument.TypeOfViewModel, argument.TypeOfSource);
            dynamic mapper = _resolutionRoot.Get(mapperTyp);

            var ret = new List<object>();
            for (var i = 0; i < argument.Source.Count; i++)
            {
                dynamic source = argument.Source[i];
                dynamic vm = argument.ViewModel != null ? argument.ViewModel[i] : null;
                ret.Add(mapper.ToViewModel(vm, source));
            }
            return ret;
        }
    }

    public class ToViewModelDynamicQueryHandler : QueryHandlerInterface<ToViewModelDynamicQuery, object>
    {
        private readonly IResolutionRoot _resolutionRoot;
        private readonly Type _genericViewModelMapperTyp = typeof(ViewModelMapperInterface<,>);

        public ToViewModelDynamicQueryHandler(IResolutionRoot resolutionRoot)
        {
            _resolutionRoot = resolutionRoot;
        }

        public object Query(ToViewModelDynamicQuery argument)
        {
            var mapperTyp = _genericViewModelMapperTyp.MakeGenericType(argument.TypeOfViewModel, argument.TypeOfSource);
            dynamic mapper = _resolutionRoot.Get(mapperTyp);
            dynamic source = argument.Source;
            dynamic vm = argument.ViewModel;


            return mapper.ToViewModel(vm, source);
        }
    }
}
