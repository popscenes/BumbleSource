using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Website.Infrastructure.Configuration;

namespace Website.Common.ApiInfrastructure.Model
{
    [DataContract]
    public class PageRequestModel
    {
        public PageRequestModel()
        {
            int count;
            Take = int.TryParse(Config.Instance.GetSetting("PageSize"), out count) ? count : 40;
        }

        [Range(0, 100)]
        public int Take { get; set; }

        [Range(0, 1000)]
        public int Skip { get; set; }
    }
}