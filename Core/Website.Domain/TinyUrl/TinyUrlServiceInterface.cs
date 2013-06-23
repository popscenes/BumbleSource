using Website.Infrastructure.Domain;

namespace Website.Domain.TinyUrl
{
    public interface TinyUrlServiceInterface
    {
        string UrlFor<UrlEntityType>(UrlEntityType entity) where UrlEntityType : class, EntityWithTinyUrlInterface, new();
        EntityInterface EntityInfoFor(string url);
    }
}
