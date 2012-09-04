using Website.Domain.Claims;

namespace Website.Domain.Service
{
    public interface PublicationServiceInterface
    {
        void Publish<PublishType>(PublishType subject);
    }
}