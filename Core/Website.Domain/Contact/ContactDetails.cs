using System;

namespace Website.Domain.Contact
{
    [Serializable]
    public class ContactDetails : ContactDetailsInterface
    {
        public ContactDetails()
        {
            
        }

        public ContactDetails(ContactDetailsInterface source)
        {
            this.CopyFieldsFrom(source);
        }

        public string PhoneNumber { get; set; }
        public string EmailAddress { get; set; }
        public string FirstName { get; set; }
        public string MiddleNames { get; set; }
        public string Surname { get; set; }
        public Location.Location Address { get; set; }
        public string WebSite { get; set; }
    }
}