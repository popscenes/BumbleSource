using System;
using Website.Domain.Browser;
using Website.Infrastructure.Domain;
using Website.Infrastructure.Query;

namespace Website.Domain.Claims
{
    public interface ClaimableEntityInterface : EntityInterface
    {
        int NumberOfClaims { get; set; }
    }

    public static class ClaimableEntityInterfaceExtensions
    {
        public static bool BrowserHasClaimed(this ClaimableEntityInterface entity, BrowserInterface browser,
                                     GenericQueryServiceInterface queryService)
        {
            var claim = new Claim() {AggregateId = entity.Id, BrowserId = browser.Id};
            return queryService.FindByAggregate<Claim>(claim.GetId(), entity.Id) != null;
        }

        public static void CopyFieldsFrom(this ClaimableEntityInterface target, ClaimableEntityInterface source)
        {
            target.NumberOfClaims = source.NumberOfClaims;
        }
    }
}