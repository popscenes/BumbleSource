using System;

namespace Website.Application.Domain.Content
{
    public interface UrlContentRetrieverInterface
    {
        Website.Domain.Content.Content GetContent(String url);
    }
}
