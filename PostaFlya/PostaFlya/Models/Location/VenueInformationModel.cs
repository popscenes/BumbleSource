using System.ComponentModel.DataAnnotations;
using PostaFlya.Domain.Venue;
using Website.Application.Domain.Location;
using Website.Common.Model;
using Website.Domain.Contact;

namespace PostaFlya.Models.Location
{
    public class ToVenueInformationModel : ViewModelMapperInterface<VenueInformationModel, VenueInformation>
    {
        public VenueInformationModel ToViewModel(VenueInformationModel target, VenueInformation source)
        {
            if (source == null)
                return null;
            var ret = target ?? new VenueInformationModel();
            ret.CopyFieldsFrom((ContactDetailFieldsInterface)source);
            ret.CopyFieldsFrom((VenueInformationFieldsInterface)source);
            ret.Address = source.Address.ToViewModel();
            return ret;
        }
    }

    public static class VenuInformationInterfaceExtensions
    {
        public static VenueInformationModel ToViewModel(this VenueInformationInterface source)
        {
            if (source == null)
                return null;
            var ret = new VenueInformationModel();
            ret.CopyFieldsFrom((ContactDetailFieldsInterface)source);
            ret.CopyFieldsFrom((VenueInformationFieldsInterface)source);
            ret.Address = source.Address.ToViewModel();
            return ret;
        }
    }


    public class VenueInformationModel : ContactDetailFieldsInterface, VenueInformationFieldsInterface
    {
        [Display(ResourceType = typeof (Properties.Resources), Name = "FlyerContactDetailsModel_PhoneNumber_FlyerContactDetailsModel_PhoneNumber_FlyerContactDetailsModel_PhoneNumber")]
        public string PhoneNumber { get; set; }

        [Display(ResourceType = typeof (Properties.Resources), Name = "FlyerContactDetailsModel_EmailAddress_FlyerContactDetailsModel_EmailAddress_FlyerContactDetailsModel_EmailAddress")]
        [StringLength(100, ErrorMessageResourceName = "StringTooLarge", ErrorMessageResourceType = typeof(Properties.Resources), ErrorMessage = null)]
        public string EmailAddress { get; set; }

        [Display(ResourceType = typeof (Properties.Resources), Name = "FlyerContactDetailsModel_FirstName_FlyerContactDetailsModel_FirstName_FlyerContactDetailsModel_FirstName")]
        [StringLength(100, ErrorMessageResourceName = "StringTooLarge", ErrorMessageResourceType = typeof(Properties.Resources), ErrorMessage = null)]
        public string FirstName { get; set; }

        [Display(ResourceType = typeof (Properties.Resources), Name = "FlyerContactDetailsModel_MiddleNames_FlyerContactDetailsModel_MiddleNames_FlyerContactDetailsModel_MiddleNames")]
        [StringLength(100, ErrorMessageResourceName = "StringTooLarge", ErrorMessageResourceType = typeof(Properties.Resources), ErrorMessage = null)]
        public string MiddleNames { get; set; }

        [Display(ResourceType = typeof (Properties.Resources), Name = "FlyerContactDetailsModel_Surname_FlyerContactDetailsModel_Surname_FlyerContactDetailsModel_Surname")]
        [StringLength(100, ErrorMessageResourceName = "StringTooLarge", ErrorMessageResourceType = typeof(Properties.Resources), ErrorMessage = null)]
        public string Surname { get; set; }

        [ValidLocation(ErrorMessageResourceName = "ValidLocation", ErrorMessageResourceType = typeof(Properties.Resources), ErrorMessage = null)]
        [Display(ResourceType = typeof (Properties.Resources), Name = "FlyerContactDetailsModel_Address_FlyerContactDetailsModel_Address_FlyerContactDetailsModel_Address")]
        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Properties.Resources), ErrorMessage = null)]
        public LocationModel Address { get; set; }

        [Display(ResourceType = typeof (Properties.Resources), Name = "FlyerContactDetailsModel_WebSite_FlyerContactDetailsModel_WebSite_FlyerContactDetailsModel_WebSite")]
        [Url(ErrorMessageResourceName = "ValidUrl", ErrorMessageResourceType = typeof(Properties.Resources), ErrorMessage = null)]
        public string WebSite { get; set; }

        public VenueInformation ToDomainModel()
        {
            var ret = new VenueInformation();
            ret.CopyFieldsFrom((VenueInformationFieldsInterface) this);
            ret.CopyFieldsFrom((ContactDetailFieldsInterface) this);

            ret.Address = Address != null ? Address.ToDomainModel() : null;
            return ret;
        }

        public string Source { get; set; }
        public string SourceId { get; set; }
        public string SourceUrl { get; set; }
        public string SourceImageUrl { get; set; }
        public string BoardFriendlyId { get; set; }

        [Display(Name = "PlaceName", ResourceType = typeof(Properties.Resources))]
        public string PlaceName { get; set; }
    }
}