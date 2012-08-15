using Ninject;
using Ninject.Syntax;

namespace PostaFlya.Application.Domain.Content
{
    public class RequestContentRetrieverFactory : RequestContentRetrieverFactoryInterface
    {
        private readonly IResolutionRoot _resolver;

        public RequestContentRetrieverFactory(IResolutionRoot resolver)
        {
            _resolver = resolver;
        }

        #region Implementation of RequestContentRetrieverFactoryInterface

        public RequestContentRetrieverInterface GetRetriever(PostaFlya.Domain.Content.Content.ContentType contentType)
        {
            return _resolver.Get<RequestContentRetrieverInterface>(md => md.Has(contentType.ToString()));
        }

        #endregion
    }
}
