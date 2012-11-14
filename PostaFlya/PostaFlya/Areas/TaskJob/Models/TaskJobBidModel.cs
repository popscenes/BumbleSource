using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Website.Application.Extension.Validation;

namespace PostaFlya.Areas.TaskJob.Models
{
    public class TaskJobBidModel
    {
        public string TaskJobId { get; set; }

        [Display(Name = "BidAmount", ResourceType = typeof(Properties.Resources))] 
        [Required]
        [RangeWithMessage(0, 1000000)]
        public double BidAmount { get; set; }
    }
}