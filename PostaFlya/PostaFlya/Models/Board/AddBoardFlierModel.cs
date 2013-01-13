using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Website.Application.Extension.Validation;

namespace PostaFlya.Models.Board
{
    [DataContract]
    public class AddBoardFlierModel : ViewModelBase
    {
        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Properties.Resources))]
        [DataMember]
        public string FlierId { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Properties.Resources))]
        [DataMember]
        public string BoardId { get; set; }
    }
}