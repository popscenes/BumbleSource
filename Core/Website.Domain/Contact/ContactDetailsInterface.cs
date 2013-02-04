using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Website.Domain.Location;

namespace Website.Domain.Contact
{
    public static class ContactDetailsInterfaceExtensions
    {
        public static void CopyFieldsFrom(this ContactDetailFieldsInterface target, ContactDetailFieldsInterface source)
        {
            if (target == null)
                return;
            target.EmailAddress = source.EmailAddress;
            target.FirstName = source.FirstName;
            target.MiddleNames = source.MiddleNames;
            target.Surname = source.Surname;
            target.PhoneNumber = source.PhoneNumber;
            target.WebSite = source.WebSite;
        }

        public static void CopyFieldsFrom<AddressType>(this ContactDetailsInterface target, ContactDetailsInterface source)
        {
            CopyFieldsFrom(target, (ContactDetailFieldsInterface)source);
            target.Address = source.Address != null ? new Location.Location(source.Address) : null;
        }
    }

    public interface ContactDetailFieldsInterface
    {
        string PhoneNumber { get; set; }
        string EmailAddress { get; set; }
        string FirstName { get; set; }
        string MiddleNames { get; set; }
        string Surname { get; set; }        
        string WebSite { get; set; }
    }

    public interface ContactDetailsInterface : ContactDetailFieldsInterface
    {
        Location.Location Address { get; set; }
    }

}
