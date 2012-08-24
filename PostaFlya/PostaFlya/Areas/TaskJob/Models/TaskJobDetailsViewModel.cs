using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using WebSite.Application.Binding;
using PostaFlya.Areas.Default.Models;
using PostaFlya.Areas.Default.Models.Bulletin;
using PostaFlya.Areas.TaskJob.Models.Bulletin;
using PostaFlya.Domain.Behaviour;
using PostaFlya.Models.Flier;
using WebSite.Application.Content;
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
                    .GetDefaultImageUrl(blobStorage, ThumbOrientation.Horizontal),
                CostOverhead = behaviour.CostOverhead
            };
        }
    }

    public class TaskJobDetailsViewModel : DefaultDetailsViewModel
    {
        [DisplayName("CostOverhead")]
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