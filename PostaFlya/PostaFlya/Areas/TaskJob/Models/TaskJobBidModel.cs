using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using WebSite.Application.Extension.Validation;

namespace PostaFlya.Areas.TaskJob.Models
{
    public class TaskJobBidModel
    {
        public string TaskJobId { get; set; }

        [DisplayName("BidAmount")]
        [Required]
        [RangeWithMessage(0, 1000000)]
        public double BidAmount { get; set; }
    }
}