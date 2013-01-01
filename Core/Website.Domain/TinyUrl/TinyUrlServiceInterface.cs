using Website.Infrastructure.Domain;

namespace Website.Domain.TinyUrl
{
    public interface TinyUrlServiceInterface
    {
        string UrlFor<UrlEntityType>(UrlEntityType entity) where UrlEntityType : TinyUrlInterface, EntityInterface;
        EntityInterface EntityInfoFor(string url);
    }
}
