using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Website.Domain.Contact;
using Website.Domain.Location;

namespace PostaFlya.Domain.Venue
{

    public static class VenuInformationFieldsExtensions 
    {
        public static void CopyFieldsFrom(this VenueInformationFieldsInterface target, VenueInformationFieldsInterface source)
        {
            if (target == null)
                return;

            PlaceInterfaceExtensions.CopyFieldsFrom(target, source);
            target.Source = source.Source;
            target.SourceId = source.SourceId;
            target.SourceUrl = source.SourceUrl;
        }
    }
    public interface VenueInformationFieldsInterface : PlaceInterface
    {
        string Source { get; set; }
        string SourceId { get; set; }
        string SourceUrl { get; set; }
    }


    public static class VenueInformationInterfaceExtensions
    {

        public static void CopyFieldsFrom(this VenueInformationInterface target, VenueInformationInterface source)
        {
            if (target == null)
                return;

            ContactDetailsInterfaceExtensions.CopyFieldsFrom(target, source);
            VenuInformationFieldsExtensions.CopyFieldsFrom(target, source);
        }
    }


    public interface VenueInformationInterface : VenueInformationFieldsInterface, ContactDetailsInterface 
    {

    }

    public class VenueInformation : VenueInformationInterface
    {

        public VenueInformation(VenueInformationInterface source)
        {
            this.CopyFieldsFrom(source);
        }

        public VenueInformation()
        {
        }

        public string PhoneNumber { get; set; }
        public string EmailAddress { get; set; }
        public string FirstName { get; set; }
        public string MiddleNames { get; set; }
        public string Surname { get; set; }
        public string WebSite { get; set; }
        public Location Address { get; set; }
        public string Source { get; set; }
        public string SourceId { get; set; }
        public string SourceUrl { get; set; }
        public string PlaceName { get; set; }
    }
}
