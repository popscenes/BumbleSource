using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Website.Application.Binding;
using PostaFlya.Areas.Default.Models;
using PostaFlya.Areas.Default.Models.Bulletin;
using PostaFlya.Areas.TaskJob.Models.Bulletin;
using PostaFlya.Domain.Behaviour;
using PostaFlya.Models.Flier;
using Website.Application.Content;
using Website.Application.Domain.Content;

namespace PostaFlya.Areas.TaskJob.Models
{
    public static class TaskJobDetailsViewModelExtension
    {
        public static TaskJobDetailsViewModel ToTaskJobDetailsViewModel(this Domain.TaskJob.TaskJobFlierBehaviourInterface behaviour
            , [ImageStorage]BlobStorageInterface blobStorage)
        {
            return new TaskJobDetailsViewModel()
            {
                Flier = behaviour.Flier.ToViewModel<BulletinTaskJobBehaviourModel>(true)
                    .GetImageUrl(blobStorage, ThumbOrientation.Horizontal),
                CostOverhead = behaviour.CostOverhead
            };
        }
    }

    public class TaskJobDetailsViewModel : DefaultDetailsViewModel
    {
        [Display(Name = "CostOverhead", ResourceType = typeof(Properties.Resources))] 
        public double CostOverhead { get; set; }

        public new static TaskJobDetailsViewModel DefaultForTemplate()
        {
            return new TaskJobDetailsViewModel()
            {
                Flier = BulletinFlierModel<BulletinTaskJobBehaviourModel>.DefaultForTemplate(FlierBehaviour.TaskJob)
            };
        }
    }
}