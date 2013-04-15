using System.ComponentModel.DataAnnotations;
using Website.Application.Domain.Location;
using Website.Domain.Contact;
using Resources = Website.Application.Properties.Resources;

namespace PostaFlya.Models.Location
{
    public static class ContactDetailsInterfaceExtensions
    {
        public static ContactDetailsModel ToViewModel(this ContactDetailsInterface source)
        {
            if (source == null)
                return null;
            var ret = new ContactDetailsModel();
            ret.CopyFieldsFrom(source);
            ret.Address = source.Address.ToViewModel();
            return ret;
        }

        public static FlyerContactDetailsModel ToFlyerContactDetailsViewModel(this ContactDetailsInterface source)
        {
            if (source == null)
                return null;
            var ret = new FlyerContactDetailsModel();
            ret.CopyFieldsFrom(source);
            ret.Address = source.Address.ToViewModel();
            return ret;
        }
  
  
    }
    public class FlyerContactDetailsModel : ContactDetailFieldsInterface
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
        [UrlAttribute(ErrorMessageResourceName = "ValidUrl", ErrorMessageResourceType = typeof(Properties.Resources), ErrorMessage = null)]
        public string WebSite { get; set; }

        public ContactDetails ToDomainModel()
        {
            var ret = new ContactDetails();
            ret.CopyFieldsFrom(this);
            ret.Address = Address != null ? Address.ToDomainModel() : null;
            return ret;
        }
    }

    public class ContactDetailsModel : ContactDetailFieldsInterface 
    {
        [Display(Name = "ContactDetailsModel_PhoneNumber", ResourceType = typeof(Properties.Resources))]
        public string PhoneNumber { get; set; }
        
        [Display(Name = "ContactDetailsModel_EmailAddress", ResourceType = typeof(Properties.Resources))]
        [StringLength(100, ErrorMessageResourceName = "StringTooLarge", ErrorMessageResourceType = typeof(Properties.Resources), ErrorMessage = null)]
        [EmailAddress(ErrorMessageResourceName = "InvalidEmailAddress", ErrorMessageResourceType = typeof(Properties.Resources), ErrorMessage = null)]
        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Properties.Resources), ErrorMessage = null)]
        public string EmailAddress { get; set; }

        [Display(Name = "ContactDetailsModel_FirstName", ResourceType = typeof(Properties.Resources))]
        [StringLength(100, ErrorMessageResourceName = "StringTooLarge", ErrorMessageResourceType = typeof(Properties.Resources), ErrorMessage = null)]
        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Properties.Resources), ErrorMessage = null)]
        public string FirstName { get; set; }

        [Display(Name = "ContactDetailsModel_MiddleNames", ResourceType = typeof(Properties.Resources))]
        [StringLength(100, ErrorMessageResourceName = "StringTooLarge", ErrorMessageResourceType = typeof(Properties.Resources), ErrorMessage = null)]
        public string MiddleNames { get; set; }

        [Display(Name = "ContactDetailsModel_Surname", ResourceType = typeof(Properties.Resources))]
        [StringLength(100, ErrorMessageResourceName = "StringTooLarge", ErrorMessageResourceType = typeof(Properties.Resources), ErrorMessage = null)]
        public string Surname { get; set; }

        [ValidLocation(ErrorMessageResourceName = "ValidLocation", ErrorMessageResourceType = typeof(Properties.Resources), ErrorMessage = null)]
        [Display(Name = "ContactDetailsModel_Address", ResourceType = typeof(Properties.Resources))]
        public LocationModel Address { get; set; }

        [Display(Name = "ContactDetailsModel_WebSite", ResourceType = typeof(Properties.Resources))]
        [UrlAttribute(ErrorMessageResourceName = "ValidUrl", ErrorMessageResourceType = typeof(Properties.Resources), ErrorMessage = null)]        
        public string WebSite { get; set; }

        public ContactDetails ToDomainModel()
        {
            var ret = new ContactDetails();
            ret.CopyFieldsFrom(this);
            ret.Address = Address != null ? Address.ToDomainModel() : null;
            return ret;
        }
    }
}