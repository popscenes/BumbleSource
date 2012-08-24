
namespace Website.Application.Domain.Content
{
    public interface UrlContentRetrieverFactoryInterface
    {
        UrlContentRetrieverInterface GetRetriever(Website.Domain.Content.Content.ContentType contentType);
    }
}
