namespace Website.Domain.Location
{
    public static class LocationAndAddressInterfaceExtensions
    {
        public static void CopyFieldsFrom(this LocationAndAddressInterface addressTarget, LocationAndAddressInterface addressSource)
        {
            LocationInterfaceExtensions.CopyFieldsFrom(addressTarget, addressSource);
            AddressInterfaceExtensions.CopyFieldsFrom(addressTarget, addressSource);
        }
    }

    public interface LocationAndAddressInterface : LocationInterface, AddressInterface
    {
        
    }
}