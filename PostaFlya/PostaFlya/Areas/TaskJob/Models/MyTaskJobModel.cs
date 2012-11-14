using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
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
        [Display(Name = "MaxTaskAmount", ResourceType = typeof(Properties.Resources))] 
        public double MaxAmount { get; set; }

        [Display(Name = "ExtraLocations", ResourceType = typeof(Properties.Resources))] 
        public List<LocationModel> ExtraLocations { get; set; }

        [Display(Name = "CostOverhead", ResourceType = typeof(Properties.Resources))] 
        public double CostOverhead { get; set; }

        public BulletinFlierModel Flier { get; set; } 
    }
}