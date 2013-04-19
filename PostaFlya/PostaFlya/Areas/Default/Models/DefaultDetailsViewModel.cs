﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using PostaFlya.Areas.Default.Models.Bulletin;
using PostaFlya.Areas.TaskJob.Models;
using PostaFlya.Domain.Behaviour;
using PostaFlya.Domain.Flier.Analytic;
using PostaFlya.Models.Flier;
using PostaFlya.Models.Location;

namespace PostaFlya.Areas.Default.Models
{
    [KnownType(typeof(TaskJobDetailsViewModel))]
    public class DefaultDetailsViewModel
    {
        public BulletinFlierModel Flier { get; set; }
        public FlierAnalyticInfoModel AnalyticInfo { get; set; }
        public VenueInformationModel VenueInformation { get; set; }
        public static DefaultDetailsViewModel DefaultForTemplate()
        {
            var ret = new DefaultDetailsViewModel()
                       {
                           Flier = BulletinFlierModel<BulletinBehaviourModel>.DefaultForTemplate(FlierBehaviour.Default),
                           AnalyticInfo = FlierAnalyticInfoModel.DefaultForTemplate(),
                           VenueInformation = new VenueInformationModel() { Address = new LocationModel() }
                       };
            return ret;
        }
    }
}