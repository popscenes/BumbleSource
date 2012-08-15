using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject;
using Ninject.Syntax;

namespace PostaFlya.Application.Domain.Content
{
    public class UrlContentRetrieverFactory : UrlContentRetrieverFactoryInterface
    {
        private readonly IResolutionRoot _resolver;

        public UrlContentRetrieverFactory(IResolutionRoot resolver)
        {
            _resolver = resolver;
        }

        public UrlContentRetrieverInterface GetRetriever(PostaFlya.Domain.Content.Content.ContentType contentType)
        {
            return _resolver.Get<UrlContentRetrieverInterface>(md => md.Has(contentType.ToString()));
        }
    }
}
