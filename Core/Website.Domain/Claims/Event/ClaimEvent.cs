using System;
using Website.Infrastructure.Domain;

namespace Website.Domain.Claims.Event
{
    [Serializable]
    public class ClaimEvent : EntityModifiedDomainEvent<Claim>
    {
    }
}
