using Website.Domain.Contact;
using Website.Domain.Location;

namespace Website.Application.Domain.Email.VCard
{
    public static class ContactDetailsInterfaceExtensions
    {
        public static Application.Email.VCard.VCard ToVCard(this ContactDetailsInterface dets)
        {
            var vcard = new Application.Email.VCard.VCard
                {
                    FirstName = dets.FirstName,
                    LastName = dets.Surname,
                    Email = dets.EmailAddress,
                    Phone = dets.PhoneNumber,
                    HomePage = dets.WebSite
                };

            if (dets.Address != null && dets.Address.HasAddressParts())
            {
                AddressInterface add = dets.Address;
                vcard.StreetAddress = !string.IsNullOrWhiteSpace(add.StreetNumber) ? add.StreetNumber + " " + add.Street : add.Street;
                vcard.Locality = add.Locality;
                vcard.Region = add.Region;
                vcard.PostalCode = add.PostCode;
                vcard.CountryName = add.CountryName;           
            }

            return vcard;
        }
    }
}