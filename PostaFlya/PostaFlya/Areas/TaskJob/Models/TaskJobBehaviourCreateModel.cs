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
        [Range(0, 1000000, ErrorMessageResourceName = "OutOfRange", ErrorMessageResourceType = typeof(Properties.Resources), ErrorMessage = null)]        
        [Display(Name = "MaxTaskAmount", ResourceType = typeof(Properties.Resources))] 
        public double MaxAmount { get; set; }

        [ValidLocations]
        public List<LocationModel> ExtraLocations { get; set; }

        [Range(0, 1000000, ErrorMessageResourceName = "OutOfRange", ErrorMessageResourceType = typeof(Properties.Resources), ErrorMessage = null)]
        public double CostOverhead { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Properties.Resources))]
        public string FlierId{ get; set; }
    }
}