using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using PostaFlya.Domain.Boards;
using PostaFlya.Models.Location;
using Website.Common.Model;
using Website.Infrastructure.Query;

namespace PostaFlya.Areas.MobileApi.Flyers.Model
{

    public class  ToFlyerBoardSummaryModel : ViewModelMapperInterface<FlyerBoardSummaryModel, Board>
    {
        private readonly QueryChannelInterface _queryChannel;

        public ToFlyerBoardSummaryModel(QueryChannelInterface queryChannel)
        {
            _queryChannel = queryChannel;
        }

        public FlyerBoardSummaryModel ToViewModel(FlyerBoardSummaryModel target, Board source)
        {
            if (target == null)
                target = new FlyerBoardSummaryModel();

            target.Location = source.Venue() != null ? source.Venue().Address.ToViewModel() : null;
            target.BoardId = source.Id;
            target.BoardName = source.Name;
            target.FriendlyId = source.FriendlyId;

            return target;

        }
    }

    [Serializable]
    [DataContract]
    public class FlyerBoardSummaryModel : IsModelInterface
    {
        [DataMember]
        public string BoardName { get; set; }

        [DataMember]
        public string BoardId { get; set; }

        [DataMember]
        public LocationModel Location { get; set; }

        [DataMember]
        public string FriendlyId { get; set; }
    }
}