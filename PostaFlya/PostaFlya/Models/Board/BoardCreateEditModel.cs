using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using PostaFlya.Domain.Boards;
using PostaFlya.Models.Location;
using Website.Application.Extension.Validation;

namespace PostaFlya.Models.Board
{
    [DataContract]
    public class BoardCreateEditModel : ViewModelBase
    {
        public string Id { get; set; }

        [Display(Name = "BoardCreateEditModel_BoardName", ResourceType = typeof(Properties.Resources))]
        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Properties.Resources))]
        [DataMember]
        public string BoardName { get; set; }

        [Display(Name = "BoardCreateEditModel_AllowOthersToPostFliers", ResourceType = typeof(Properties.Resources))] 
        [DataMember]
        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Properties.Resources))]
        public bool AllowOthersToPostFliers { get; set; }

        [Display(Name = "BoardCreateEditModel_RequireApprovalOfPostedFliers", ResourceType = typeof(Properties.Resources))] 
        [DataMember]
        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Properties.Resources))]
        public bool RequireApprovalOfPostedFliers { get; set; }

//        [Display(Name = "BoardCreateEditModel_Location", ResourceType = typeof(Properties.Resources))] 
//        public LocationModel Location { get; set; }

        [Display(Name = "BoardCreateEditModel_Description", ResourceType = typeof(Properties.Resources))] 
        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Properties.Resources))]
        public string Description { get; set; }

        [Display(Name = "BoardCreateEditModel_TypeOfBoard", ResourceType = typeof(Properties.Resources))]
        [DataMember(IsRequired = true)]
        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Properties.Resources))]
        public BoardTypeEnum TypeOfBoard { get; set; }

        [Display(Name = "BoardCreateEditModel_Status", ResourceType = typeof (Properties.Resources))] 
        public BoardStatus? Status { get; set; }
    }
}