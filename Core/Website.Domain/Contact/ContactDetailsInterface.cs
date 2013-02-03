using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Website.Domain.Location;

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
            target.PhoneNumber = source.PhoneNumber;
            target.WebSite = source.WebSite;
        }
    }

    public static class ContactLocationInterfaceExtensions
    {
        public static void CopyFieldsFrom<LocationType>(this ContactLocationInterface<LocationType> target, ContactLocationInterface<LocationType> source) where LocationType : class, LocationAndAddressInterface, new()
        {
            if (source.Address != null)
            {
                target.Address = new LocationType();
                target.Address.CopyFieldsFrom((AddressInterface)source.Address);
                target.Address.CopyFieldsFrom((LocationInterface)source.Address);
            }
            else
            {
                target.Address = null;
            }
            target.Address =  ? new Location.Location(source.Address) : null;
        }
    }

    public interface ContactLocationInterface<LocationType> where LocationType : LocationAndAddressInterface
    {
        LocationType Address { get; set; }
    }

    public interface ContactDetailsInterface
    {
        string PhoneNumber { get; set; }
        string EmailAddress { get; set; }
        string FirstName { get; set; }
        string MiddleNames { get; set; }
        string Surname { get; set; }
        
        string WebSite { get; set; }
        
    }
}
