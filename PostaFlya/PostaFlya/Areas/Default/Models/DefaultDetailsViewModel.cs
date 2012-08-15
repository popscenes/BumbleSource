using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PostaFlya.Areas.Default.Models.Bulletin;
using PostaFlya.Domain.Behaviour;
using PostaFlya.Models.Flier;

namespace PostaFlya.Areas.Default.Models
{
    public class DefaultDetailsViewModel
    {
        public BulletinFlierModel Flier { get; set; }
        public static DefaultDetailsViewModel DefaultForTemplate()
        {
            var ret = new DefaultDetailsViewModel()
                       {
                           Flier = BulletinFlierModel<BulletinBehaviourModel>.DefaultForTemplate(FlierBehaviour.Default)
                       };
            return ret;
        }
    }
}