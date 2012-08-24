using Ninject;
using Ninject.Syntax;

namespace Website.Application.Domain.Content
{
    public class UrlContentRetrieverFactory : UrlContentRetrieverFactoryInterface
    {
        private readonly IResolutionRoot _resolver;

        public UrlContentRetrieverFactory(IResolutionRoot resolver)
        {
            _resolver = resolver;
        }

        public UrlContentRetrieverInterface GetRetriever(Website.Domain.Content.Content.ContentType contentType)
        {
            return _resolver.Get<UrlContentRetrieverInterface>(md => md.Has(contentType.ToString()));
        }
    }
}
