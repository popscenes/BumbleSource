using System.Runtime.Serialization;
using PostaFlya.Controllers;
using PostaFlya.Models.Location;

namespace PostaFlya.Models.Board
{
    [DataContract]
    public class BoardSearchModel : PagedModel
    {
        [DataMember]
        public LocationModel Loc { get; set; }

        [DataMember]
        public int Distance { get; set; }
    }
}