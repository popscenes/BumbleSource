using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Website.Domain.Contact
{
    public static class ContactDetailsInterfaceExtensions
    {
        public static void CopyFieldsFrom(this ContactDetailsInterface target, ContactDetailsInterface source)
        {
            if (target == null)
                return;
            target.EmailAddress = source.EmailAddress;
            target.FirstName = source.FirstName;
            target.MiddleNames = source.MiddleNames;
            target.Surname = source.Surname;
            target.Address = source.Address != null ? new Location.Location(source.Address) : null;
            target.PhoneNumber = source.PhoneNumber;
            target.WebSite = source.WebSite;
        }
    }

    public interface ContactDetailsInterface
    {
        string PhoneNumber { get; set; }
        string EmailAddress { get; set; }
        string FirstName { get; set; }
        string MiddleNames { get; set; }
        string Surname { get; set; }
        Location.Location Address { get; set; }
        string WebSite { get; set; }
        
    }
}
