using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using PostaFlya.Domain.Flier;
using Website.Common.Model;
using Website.Common.Model.Query;
using Website.Infrastructure.Query;
using Res = PostaFlya.Properties.Resources;

namespace PostaFlya.Areas.WebApi.Flyers.Model
{
    public class ToFlyerDetailModel : ViewModelMapperInterface<FlyerDetailModel, Flier>
    {
        private readonly QueryChannelInterface _queryChannel;

        public ToFlyerDetailModel(QueryChannelInterface queryChannel)
        {
            _queryChannel = queryChannel;
        }

        public FlyerDetailModel ToViewModel(FlyerDetailModel target, Flier source)
        {
            if (target == null)
                target = new FlyerDetailModel();

            _queryChannel.ToViewModel<FlyerSummaryModel, Flier>(source, target);
            
            target.Description = source.Description;
            
            return target;

        }
    }

    [Serializable]
    [DataContract]
    public class FlyerDetailModel : FlyerSummaryModel
    {
        [DataMember]
        [Display(Name = "Flyer_Description", ResourceType = typeof(Res))]
        public string Description { get; set; }

    }
}