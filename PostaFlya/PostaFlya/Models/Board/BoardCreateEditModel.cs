using System.ComponentModel;
using System.Runtime.Serialization;
using PostaFlya.Models.Location;
using Website.Application.Extension.Validation;

namespace PostaFlya.Models.Board
{
    [DataContract]
    public class BoardCreateEditModel : ViewModelBase
    {
        public string Id { get; set; }

        [DisplayName("BoardName")]
        [RequiredWithMessage]
        public string BoardName { get; set; }

        [DisplayName("AllowOthersToPostFliers")]
        [DataMember]
        [RequiredWithMessage]
        public bool AllowOthersToPostFliers { get; set; }

        [DisplayName("RequireApprovalOfPostedFliers")]
        [DataMember]
        [RequiredWithMessage]
        public bool RequireApprovalOfPostedFliers { get; set; }

        [DisplayName("Location")]
        public LocationModel Location { get; set; }

    }
}