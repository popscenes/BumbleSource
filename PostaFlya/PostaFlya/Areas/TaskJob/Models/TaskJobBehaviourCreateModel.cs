using System.Collections.Generic;
using System.ComponentModel;
using PostaFlya.Application.Domain.Location;
using WebSite.Application.Extension.Validation;
using PostaFlya.Models;
using PostaFlya.Models.Location;

namespace PostaFlya.Areas.TaskJob.Models
{
    public class TaskJobBehaviourCreateModel : ViewModelBase
    {
        [RequiredWithMessage]
        [RangeWithMessage(1, 1000000)]
        [DisplayName("MaxTaskAmount")]
        public double MaxAmount { get; set; }

        [ValidLocations]
        public List<LocationModel> ExtraLocations { get; set; }

        [RangeWithMessage(0, 1000000)]
        public double CostOverhead { get; set; }

        [RequiredWithMessage]
        public string FlierId{ get; set; }
    }
}