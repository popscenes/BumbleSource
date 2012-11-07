using System.Runtime.Serialization;
using Website.Application.Extension.Validation;

namespace PostaFlya.Models.Board
{
    [DataContract]
    public class AddBoardFlierModel : ViewModelBase
    {
        [RequiredWithMessage]
        [DataMember]
        public string FlierId { get; set; }

        [RequiredWithMessage]
        [DataMember]
        public string BoardId { get; set; }
    }
}