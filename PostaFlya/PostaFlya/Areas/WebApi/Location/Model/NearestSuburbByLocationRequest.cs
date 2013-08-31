using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using PostaFlya.Models.Flier;
using PostaFlya.Models.Location;

namespace PostaFlya.Areas.WebApi.Location.Model
{
    [DataContract]
    public class NearestSuburbByLocationRequest : RequestModelInterface
    {
        [Range(-90.0, 90.0)]
        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Properties.Resources))]
        [DataMember(IsRequired = true)]
        public double Lat { get; set; }

        [Range(-180.0, 180.0)]
        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Properties.Resources))]
        [DataMember(IsRequired = true)]
        public double Lng { get; set; }
    }
}