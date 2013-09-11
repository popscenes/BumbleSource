using System.Runtime.Serialization;
using PostaFlya.Areas.WebApi.Location.Model;
using PostaFlya.Controllers;
using PostaFlya.Models.Location;

namespace PostaFlya.Models.Board
{
    [DataContract]
    public class BoardSearchModel : PagedModel
    {
        [DataMember]
        public SuburbModel Loc { get; set; }

        [DataMember]
        public int Distance { get; set; }
    }
}