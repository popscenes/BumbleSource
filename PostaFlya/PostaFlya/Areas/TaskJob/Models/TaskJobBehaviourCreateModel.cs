using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Website.Application.Extension.Validation;
using PostaFlya.Models;
using PostaFlya.Models.Location;
using Website.Application.Domain.Location;

namespace PostaFlya.Areas.TaskJob.Models
{
    public class TaskJobBehaviourCreateModel : ViewModelBase
    {
        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Properties.Resources))]
        [RangeWithMessage(1, 1000000)]
        [Display(Name = "MaxTaskAmount", ResourceType = typeof(Properties.Resources))] 
        public double MaxAmount { get; set; }

        [ValidLocations]
        public List<LocationModel> ExtraLocations { get; set; }

        [RangeWithMessage(0, 1000000)]
        public double CostOverhead { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Properties.Resources))]
        public string FlierId{ get; set; }
    }
}