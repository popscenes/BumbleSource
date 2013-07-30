using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using PostaFlya.Models.Flier;

namespace PostaFlya.Areas.WebApi.Flyers.Model
{
    [DataContract]
    public class FlyersByLatestRequest : RequestModelInterface
    {
        public FlyersByLatestRequest()
        {
            Take = 30;
        }

        [DataMember]
        [Range(0, 100)]
        public int Take { get; set; }

        [DataMember]
        public string Skip { get; set; }

    }
}