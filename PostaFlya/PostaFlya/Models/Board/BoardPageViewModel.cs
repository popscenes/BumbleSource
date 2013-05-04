using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using PostaFlya.Domain.Boards;
using PostaFlya.Domain.Venue;
using PostaFlya.Models.Location;
using Website.Common.Model;
using Website.Common.Model.Query;
using Website.Infrastructure.Query;

namespace PostaFlya.Models.Board
{
    public class ToBoardViewModel 
        : ViewModelMapperInterface<BoardPageViewModel, PostaFlya.Domain.Boards.Board>
    {
        private readonly QueryChannelInterface _queryChannel;

        public ToBoardViewModel(QueryChannelInterface queryChannel)
        {
            _queryChannel = queryChannel;
        }

        public BoardPageViewModel ToViewModel(BoardPageViewModel target, Domain.Boards.Board source)
        {
            if(target == null)
                target = new BoardPageViewModel();
            target.Name = source.Name;
            target.FriendlyId = source.FriendlyId;
            target.Description = source.Description;
            target.VenueInformation = source
                .InformationSources
                .Select(information => _queryChannel.ToViewModel<VenueInformationModel>(information))
                .ToList();
            target.Location = _queryChannel.ToViewModel<LocationModel>(source.Location);
            target.BoardTypeEnum = source.BoardTypeEnum;
            target.Id = source.Id;
            target.DefaultVenueInformation =
                target.VenueInformation.FirstOrDefault(model => model.Source == source.DefaultInformationSource);
            target.DefaultVenueInformation = target.DefaultVenueInformation ?? target.VenueInformation.FirstOrDefault(); 
            return target;
        }
    }

    [DataContract]
    public class BoardPageViewModel : PageModelInterface
    {
        public BoardPageViewModel()
        {
            PageId = WebConstants.BoardPage;
        }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string FriendlyId { get; set; }

        [DataMember]
        public List<VenueInformationModel> VenueInformation { get; set; }

        [DataMember]
        public VenueInformationModel DefaultVenueInformation { get; set; }

        [DataMember]
        public LocationModel Location { get; set; }

        [DataMember]
        public BoardTypeEnum BoardTypeEnum { get; set; }

        public string PageId { get; set; }
        public string ActiveNav { get; set; }
    }
}