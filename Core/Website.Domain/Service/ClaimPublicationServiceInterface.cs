using Website.Domain.Claims;

namespace Website.Domain.Service
{
    public interface ClaimPublicationServiceInterface
    {
        void Publish(Claim claim);
    }
}