using System;
using Website.Infrastructure.Domain;

namespace PostaFlya.Domain.Boards.Event
{
    [Serializable]
    public class VenueBoardModifiedEvent : EntityModifiedDomainEvent<VenueBoard>
    {
    }
}