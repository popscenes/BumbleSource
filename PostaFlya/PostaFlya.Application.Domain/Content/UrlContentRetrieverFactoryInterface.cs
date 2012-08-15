using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PostaFlya.Application.Domain.Content
{
    public interface UrlContentRetrieverFactoryInterface
    {
        UrlContentRetrieverInterface GetRetriever(PostaFlya.Domain.Content.Content.ContentType contentType);
    }
}
