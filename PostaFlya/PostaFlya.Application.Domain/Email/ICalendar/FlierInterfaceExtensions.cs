using PostaFlya.Domain.Flier;
using Website.Application.Email.ICalendar;
using Website.Domain.Browser;
using Role = Website.Application.Email.ICalendar.Role;

namespace PostaFlya.Application.Domain.Email.ICalendar
{
    public static class FlierInterfaceExtensions
    {
        public static Event ToICalEvent(this FlierInterface flier, BrowserInterface browser)
        {
            var evnt = new Event
                {
                    Title = flier.Title, 
                    StartTime = flier.EffectiveDate, 
                    EndTime = flier.EffectiveDate.AddDays(1),
                    Description = flier.Description,
                    IsUtcTime = true
                };

            var contact = flier.GetContactDetailsForFlier(browser);
            if(contact != null)
            {
                evnt.Organizer = new Organizer
                    {
                        PublicName = browser.GetNameString(), 
                        Email = contact.EmailAddress, 
                        Role = Role.CHAIR
                    };
            }

            evnt.GpsCoordinate = new GeoCoordinate(flier.Location.Latitude, flier.Location.Longitude);
            evnt.Status = EventStatus.CONFIRMED;
            evnt.PriorityLevel = PriorityLevel.Normal;
            evnt.SequenceNbr = 1;
            evnt.Location = flier.Location.Description;
            evnt.UID = flier.Id;
            return evnt;
        }
    }
}