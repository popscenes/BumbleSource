using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using PostaFlya.Models.Location;
using Website.Application.Extension.Validation;
using Resources = PostaFlya.Properties.Resources;

namespace PostaFlya.Models.Board
{
    [DataContract]
    public class BoardCreateEditModel : ViewModelBase
    {
        public string Id { get; set; }

        [Display(Name = "BoardCreateEditModel_BoardName", ResourceType = typeof(Properties.Resources))] 
        [RequiredWithMessage]
        public string BoardName { get; set; }

        [Display(Name = "BoardCreateEditModel_AllowOthersToPostFliers", ResourceType = typeof(Properties.Resources))] 
        [DataMember]
        [RequiredWithMessage]
        public bool AllowOthersToPostFliers { get; set; }

        [Display(Name = "BoardCreateEditModel_RequireApprovalOfPostedFliers", ResourceType = typeof(Properties.Resources))] 
        [DataMember]
        [RequiredWithMessage]
        public bool RequireApprovalOfPostedFliers { get; set; }

        [Display(Name = "BoardCreateEditModel_Location", ResourceType = typeof(Properties.Resources))] 
        public LocationModel Location { get; set; }

        [Display(Name = "BoardCreateEditModel_Description", ResourceType = typeof (Resources))] 
        [RequiredWithMessage]
        public string Description { get; set; }

        [Display(Name = "BoardCreateEditModel_PercentageOfPublicFliersToShow", ResourceType = typeof (Resources))] 
        public int PercentageOfPublicFliersToShow { get; set; }   
    }
}