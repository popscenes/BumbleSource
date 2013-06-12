using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Website.Application.Domain.Location;

namespace PostaFlya.Areas.MobileApi.Flyers.Model
{
    //lat=-37.769&long=144.979&distance=10&take=30 
    [DataContract]
    public class FlyersByLocationRequest
    {
        public FlyersByLocationRequest()
        {
            Distance = 5;
            Take = 30;
        }

        [ValidLatitude]
        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Properties.Resources))]
        [DataMember(IsRequired = true)]
        public double Lat { get; set; }

        [ValidLongitude]
        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Properties.Resources))]
        [DataMember(IsRequired = true)]
        public double Long { get; set; }

        public int Distance { get; set; }

        public int Take { get; set; }
    }
}