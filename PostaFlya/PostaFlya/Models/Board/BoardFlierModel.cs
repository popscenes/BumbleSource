using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using PostaFlya.Domain.Boards;
using PostaFlya.Models.Flier;

namespace PostaFlya.Models.Board
{
    [DataContract]
    public class BoardFlierModel
    {
        [Display(Name = "BoardFlier", ResourceType = typeof(Properties.Resources))] 
        [DataMember]        
        public BulletinFlierModel BoardFlier { get; set; }

        [Display(Name = "BoardId ", ResourceType = typeof(Properties.Resources))] 
        [DataMember]        
        public string BoardId { get; set; }
        
        [Display(Name = "Status", ResourceType = typeof(Properties.Resources))] 
        [DataMember]        
        public BoardFlierStatus Status { get; set; }
    }
}