using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using PostaFlya.Areas.Default.Models.Bulletin;
using PostaFlya.Areas.TaskJob.Models.Bulletin;
using PostaFlya.Controllers;
using PostaFlya.Models.Flier;
using PostaFlya.Models.Location;
using Website.Application.Content;

namespace PostaFlya.Areas.TaskJob.Models
{
    public static class TaskJobFlierBehaviourInterfaceExtension
    {
        public static MyTaskJobModel ToMyTaskJobModel(this Domain.TaskJob.TaskJobFlierBehaviourInterface  behaviour
            , BlobStorageInterface blobStorage)
        {
            return new MyTaskJobModel()
            {
                ExtraLocations = behaviour.ExtraLocations.Select(l => l.ToViewModel()).ToList(),
                Flier = behaviour.Flier.ToViewModel<BulletinTaskJobBehaviourModel>(true).GetImageUrl(blobStorage),
                MaxAmount = behaviour.MaxAmount,
                CostOverhead = behaviour.CostOverhead
            };
        }
    }

    public class MyTaskJobModel
    {
        [DisplayName("MaxTaskAmount")]
        public double MaxAmount { get; set; }

        [DisplayName("ExtraLocations")]
        public List<LocationModel> ExtraLocations { get; set; }

        [DisplayName("CostOverhead")]
        public double CostOverhead { get; set; }

        public BulletinFlierModel Flier { get; set; } 
    }
}