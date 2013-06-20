using System.Runtime.Serialization;

namespace PostaFlya.Areas.MobileApi.Flyers.Model
{
    [DataContract]
    public class FlyersByLatestRequest
    {
        public FlyersByLatestRequest()
        {
            Take = 30;
        }

        public int Take { get; set; }

        public string Skip { get; set; }

    }
}