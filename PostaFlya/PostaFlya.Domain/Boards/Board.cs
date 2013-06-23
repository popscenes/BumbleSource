using System;
using System.Collections.Generic;
using PostaFlya.Domain.Venue;
using Website.Domain.Location;
using Website.Infrastructure.Domain;

namespace PostaFlya.Domain.Boards
{
    [Serializable]
    public class Board : EntityBase<BoardInterface>, AggregateRootInterface, BoardInterface
    {
        public string BrowserId { get; set; }
        public bool AllowOthersToPostFliers { get; set; }
        public bool RequireApprovalOfPostedFliers { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public BoardStatus Status { get; set; }
        public BoardTypeEnum BoardTypeEnum { get; set; }
        public BoardSubscription Subscription { get; set; }
        public string ImageId { get; set; }
        public List<VenueInformation> InformationSources { get; set; }
        public string DefaultInformationSource { get; set; }
        public List<string> AdminEmailAddresses { get; set; }
    }
}