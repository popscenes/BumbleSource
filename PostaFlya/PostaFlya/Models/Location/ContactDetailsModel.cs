using System.ComponentModel.DataAnnotations;
using Website.Application.Domain.Location;
using Website.Domain.Contact;

namespace PostaFlya.Models.Location
{
    public class ContactDetailsModel : ContactDetailFieldsInterface 
    {
        [Display(Name = "ContactDetailsModel_PhoneNumber", ResourceType = typeof(Properties.Resources))]
        public string PhoneNumber { get; set; }
        
        [Display(Name = "ContactDetailsModel_EmailAddress", ResourceType = typeof(Properties.Resources))]
        [StringLength(100, ErrorMessageResourceName = "StringTooLarge", ErrorMessageResourceType = typeof(Properties.Resources), ErrorMessage = null)]
        [EmailAddress(ErrorMessageResourceName = "InvalidEmailAddress", ErrorMessageResourceType = typeof(Properties.Resources), ErrorMessage = null)]
        public string EmailAddress { get; set; }

        [Display(Name = "ContactDetailsModel_FirstName", ResourceType = typeof(Properties.Resources))]
        [StringLength(100, ErrorMessageResourceName = "StringTooLarge", ErrorMessageResourceType = typeof(Properties.Resources), ErrorMessage = null)]
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
    }
}