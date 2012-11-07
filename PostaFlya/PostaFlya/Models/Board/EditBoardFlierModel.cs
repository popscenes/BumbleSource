using System.Runtime.Serialization;
using PostaFlya.Domain.Boards;
using Website.Application.Extension.Validation;

namespace PostaFlya.Models.Board
{
    [DataContract]
    public class EditBoardFlierModel : AddBoardFlierModel
    {
        [RequiredWithMessage]
        [DataMember(IsRequired = true)]
        public BoardFlierStatus Status { get; set; }
    }
}