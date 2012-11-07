using System.ComponentModel;
using System.Runtime.Serialization;
using PostaFlya.Domain.Boards;
using PostaFlya.Models.Flier;

namespace PostaFlya.Models.Board
{
    [DataContract]
    public class BoardFlierModel
    {
        [DisplayName("Flier")]
        [DataMember]        
        public BulletinFlierModel Flier { get; set; }

        [DisplayName("BoardId ")]
        [DataMember]        
        public string BoardId { get; set; }
        
        [DisplayName("Status")]
        [DataMember]        
        public BoardFlierStatus Status { get; set; }
    }
}