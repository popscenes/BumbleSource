namespace Website.Application.Domain.Content
{
    public interface RequestContentRetrieverFactoryInterface
    {
        RequestContentRetrieverInterface GetRetriever(Website.Domain.Content.Content.ContentType contentType);
    }
}