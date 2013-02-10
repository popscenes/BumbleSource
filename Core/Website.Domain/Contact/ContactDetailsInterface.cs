using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Website.Domain.Location;
using Website.Infrastructure.Util;

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

        public static void CopyFieldsFrom(this ContactDetailsInterface target, ContactDetailsInterface source)
        {
            CopyFieldsFrom(target, (ContactDetailFieldsInterface)source);
            target.Address = source.Address != null ? new Location.Location(source.Address) : null;
        }

        public static bool HasEnoughForContact(this ContactDetailsInterface target)
        {
            return 
                   string.IsNullOrWhiteSpace(target.EmailAddress) ||
                   string.IsNullOrWhiteSpace(target.PhoneNumber) ||
                   string.IsNullOrWhiteSpace(target.WebSite);
        }

        public static bool IsSimilarTo(this ContactDetailFieldsInterface target, ContactDetailFieldsInterface source)
        {
            if (!StringUtil.AreBothEqualOrNullOrWhiteSpace(target.PhoneNumber, source.PhoneNumber))
                return false;
            if (!StringUtil.AreBothEqualOrNullOrWhiteSpace(target.EmailAddress, source.EmailAddress))
                return false;
            if (!StringUtil.AreBothEqualOrNullOrWhiteSpace(target.FirstName, source.FirstName))
                return false;
            if (!StringUtil.AreBothEqualOrNullOrWhiteSpace(target.MiddleNames, source.MiddleNames))
                return false;
            if (!StringUtil.AreBothEqualOrNullOrWhiteSpace(target.Surname, source.Surname))
                return false;
            if (!StringUtil.AreBothEqualOrNullOrWhiteSpace(target.WebSite, source.WebSite))
                return false;
            return true;
        }

        public static bool IsSimilarTo(this ContactDetailsInterface target, ContactDetailsInterface source)
        {
            if (!IsSimilarTo((ContactDetailFieldsInterface)target, source))
                return false;
            if (target.Address == null && source.Address == null)
                return true;
            if (target.Address != null && source.Address != null)
                return target.Address.IsSimilarTo(source.Address);
            return false;
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
