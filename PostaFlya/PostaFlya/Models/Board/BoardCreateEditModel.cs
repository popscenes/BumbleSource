using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using PostaFlya.Models.Location;
using Website.Application.Extension.Validation;

namespace PostaFlya.Models.Board
{
    [DataContract]
    public class BoardCreateEditModel : ViewModelBase
    {
        public string Id { get; set; }

        [Display(Name = "BoardName", ResourceType = typeof(Properties.Resources))] 
        [RequiredWithMessage]
        public string BoardName { get; set; }

        [Display(Name = "AllowOthersToPostFliers", ResourceType = typeof(Properties.Resources))] 
        [DataMember]
        [RequiredWithMessage]
        public bool AllowOthersToPostFliers { get; set; }

        [Display(Name = "RequireApprovalOfPostedFliers", ResourceType = typeof(Properties.Resources))] 
        [DataMember]
        [RequiredWithMessage]
        public bool RequireApprovalOfPostedFliers { get; set; }

        [Display(Name = "Location", ResourceType = typeof(Properties.Resources))] 
        public LocationModel Location { get; set; }

    }
}