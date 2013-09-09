using PostaFlya.Domain.Flier;
using Website.Application.Email.ICalendar;
using Website.Domain.Browser;
using Website.Domain.Location;
using Website.Infrastructure.Query;
using Role = Website.Application.Email.ICalendar.Role;

namespace PostaFlya.Application.Domain.Email.ICalendar
{
    public static class FlierInterfaceExtensions
    {
        public static Event ToICalEvent(this FlierInterface flier, QueryChannelInterface queryChannel)
        {
            var evnt = new Event
                {
                    Title = flier.Title, 
                    StartTime = flier.EffectiveDate, 
                    EndTime = flier.EffectiveDate.AddDays(1),
                    Description = flier.Description,
                    IsUtcTime = true
                };

            var contact = flier.GetVenueForFlier(queryChannel);
            if(contact != null)
            {
                evnt.Organizer = new Organizer
                    {
                        PublicName = contact.PlaceName, 
                        Email = contact.EmailAddress, 
                        Role = Role.CHAIR
                    };
            }

            evnt.GpsCoordinate = new GeoCoordinate(contact.Address.Latitude, contact.Address.Longitude);
            evnt.Status = EventStatus.CONFIRMED;
            evnt.PriorityLevel = PriorityLevel.Normal;
            evnt.SequenceNbr = 1;
            evnt.Location = contact.PlaceName + " (" + contact.Address.GetAddressDescription() +")";
            evnt.UID = flier.Id;
            return evnt;
        }
    }
}