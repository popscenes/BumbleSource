using System;

namespace Website.Domain.Contact
{
    [Serializable]
    public class ContactDetails : ContactDetailsInterface
    {
        public string PhoneNumber { get; set; }
        public string EmailAddress { get; set; }
        public string FirstName { get; set; }
        public string MiddleNames { get; set; }
        public string Surname { get; set; }
        public Location.Location Address { get; set; }
    }
}