using System.Text;

namespace Website.Domain.Location
{
    public static class AddressInterfaceExtensions
    {
        //1 Waihi Avenue, Brunswick East, VIC, 3057, Australia
        public static string GetAddressDescription(this AddressInterface address)
        {
            var addDesc = new StringBuilder();
            AddAddressPart(address.StreetAddress, addDesc);
            AddAddressPart(address.Locality, addDesc);
            AddAddressPart(address.Region, addDesc);
            AddAddressPart(address.PostCode, addDesc);
            AddAddressPart(address.CountryName, addDesc);  
            return addDesc.ToString();
        }

        private static void AddAddressPart(string addressPart, StringBuilder builder)
        {
            if (string.IsNullOrWhiteSpace(addressPart))
                return;
            builder.Append(addressPart);
            builder.Append(", ");
        }
    }

    public interface AddressInterface
    {
        string StreetAddress { get; set; }
        string Locality { get; set; }
        string Region { get; set; }
        string PostCode { get; set; }
        string CountryName { get; set; }
    }
}