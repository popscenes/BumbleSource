using System.Linq;
using System.Text;
using Website.Infrastructure.Util;
using Website.Infrastructure.Util.Extension;

namespace Website.Domain.Location
{
    public static class AddressInterfaceExtensions
    {
        public static void CopyFieldsFrom(this AddressInterface addressTarget, AddressInterface addressSource)
        {
            addressTarget.StreetNumber = addressSource.StreetNumber;
            addressTarget.Street = addressSource.Street;            
            addressTarget.Locality = addressSource.Locality;
            addressTarget.Region = addressSource.Region;
            addressTarget.PostCode = addressSource.PostCode;
            addressTarget.CountryName = addressSource.CountryName;
        }

        public static bool HasAddressParts(this AddressInterface address)
        {

            return
                    !string.IsNullOrWhiteSpace(address.StreetNumber) ||
                    !string.IsNullOrWhiteSpace(address.Street) ||
                    !string.IsNullOrWhiteSpace(address.Locality) ||
                    !string.IsNullOrWhiteSpace(address.Region) ||
                    !string.IsNullOrWhiteSpace(address.PostCode) ||
                    !string.IsNullOrWhiteSpace(address.CountryName);
        }

        //My House, 1 Waihi Avenue, Brunswick East VIC 3057, Australia
        public static string GetAddressDescription(this AddressInterface address)
        {
            if (address == null)
                return null;
            var addDesc = new StringBuilder();
            AddAddressPart(address.StreetNumber, addDesc, ", ");
            AddAddressPart(address.Street, addDesc, " ");
            AddAddressPart(address.Locality, addDesc, ", ");
            AddAddressPart(address.Region, addDesc, " ");
            AddAddressPart(address.PostCode, addDesc, " ");
            AddAddressPart(address.CountryName, addDesc, ", ");  
            return addDesc.ToString();
        }

        //[1][Waihi Avenue][Brunswick East][VIC][3057][Australia]
        public static string GetAddressStringFormat(this AddressInterface address)
        {
            return string.Format("[{0}],[{1}],[{2}],[{3}],[{4}],[{5}]"
                          , address.StreetNumber.EmptyIfNull()
                          , address.Street.EmptyIfNull()
                          , address.Locality.EmptyIfNull()
                          , address.Region.EmptyIfNull()
                          , address.PostCode.EmptyIfNull()
                          , address.CountryName.EmptyIfNull()
                );
        }

        public static void SetAddressFromStringFormat(this AddressInterface address, string format)
        {
            var parts = format.Split(',').Reverse().ToList();

            if (parts.Any())
                address.CountryName = parts[0].Trim('[', ']');

            if (parts.Count > 1)
                address.PostCode = parts[1].Trim('[', ']');

            if (parts.Count > 2)
                address.Region = parts[2].Trim('[', ']');

            if (parts.Count > 3)
                address.Locality = parts[3].Trim('[', ']');

            if (parts.Count > 4)
                address.Street = parts[4].Trim('[', ']');
            
            if (parts.Count > 5)
                address.StreetNumber = parts[5].Trim('[', ']');
             
        }

        private static void AddAddressPart(string addressPart, StringBuilder builder, string separator)
        {
            if (string.IsNullOrWhiteSpace(addressPart))
                return;
            if (builder.Length > 0)
                builder.Append(separator);
            builder.Append(addressPart);          
        }

        public static bool IsSimilarTo(this AddressInterface target, AddressInterface source)
        {

            if (!StringUtil.AreBothEqualOrNullOrWhiteSpace(target.StreetNumber, source.StreetNumber))
                return false;
            if (!StringUtil.AreBothEqualOrNullOrWhiteSpace(target.Street, source.Street))
                return false;
            if (!StringUtil.AreBothEqualOrNullOrWhiteSpace(target.Locality, source.Locality))
                return false;
            if (!StringUtil.AreBothEqualOrNullOrWhiteSpace(target.Region, source.Region))
                return false;
            if (!StringUtil.AreBothEqualOrNullOrWhiteSpace(target.PostCode, source.PostCode))
                return false;
            if (!StringUtil.AreBothEqualOrNullOrWhiteSpace(target.CountryName, source.CountryName))
                return false;
            return true;
        }
    }

    public interface AddressInterface
    {
        string StreetNumber { get; set; }
        string Street { get; set; }
        string Locality { get; set; }
        string Region { get; set; }
        string PostCode { get; set; }
        string CountryName { get; set; }
    }


}