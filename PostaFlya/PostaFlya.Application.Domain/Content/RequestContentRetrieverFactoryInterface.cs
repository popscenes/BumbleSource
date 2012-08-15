using WebSite.Application.Content;

namespace PostaFlya.Application.Domain.Content
{
    public interface RequestContentRetrieverFactoryInterface
    {
        RequestContentRetrieverInterface GetRetriever(PostaFlya.Domain.Content.Content.ContentType contentType);
    }
}