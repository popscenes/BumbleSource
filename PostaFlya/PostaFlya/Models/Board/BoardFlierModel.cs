using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using PostaFlya.Domain.Boards;
using PostaFlya.Models.Flier;
using Website.Common.Model;

namespace PostaFlya.Models.Board
{
    [Serializable]
    [DataContract]
    public class BoardFlierModel : IsModelInterface
    {
        [Display(Name = "BoardFlier", ResourceType = typeof(Properties.Resources))] 
        [DataMember]        
        public BulletinFlierSummaryModel BoardFlier { get; set; }

        [Display(Name = "BoardId ", ResourceType = typeof(Properties.Resources))] 
        [DataMember]        
        public string BoardId { get; set; }
        
        [Display(Name = "Status", ResourceType = typeof(Properties.Resources))] 
        [DataMember]        
        public BoardFlierStatus Status { get; set; }
    }
}