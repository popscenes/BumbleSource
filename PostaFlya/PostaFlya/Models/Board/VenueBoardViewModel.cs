using System.Collections.Generic;
using System.Linq;
using PostaFlya.Domain.Boards;
using PostaFlya.Models.Location;
using Website.Common.Model;
using Website.Common.Model.Query;
using Website.Infrastructure.Query;

namespace PostaFlya.Models.Board
{
    public class ToVenueBoardViewModel : ViewModelMapperInterface<BoardViewModel, VenueBoard>
    {
        private readonly QueryChannelInterface _queryChannel;

        public ToVenueBoardViewModel(QueryChannelInterface queryChannel)
        {
            _queryChannel = queryChannel;
        }

        public BoardViewModel ToViewModel(BoardViewModel target, VenueBoard source)
        {
            var ret = new VenueBoardViewModel
                {
                    Description = source.Description,
                    VenueInformation = source
                        .InformationSources.Select(information =>
                                        _queryChannel.ToViewModel<VenueInformationModel>(information)).ToList(),
                    Location = _queryChannel.ToViewModel<LocationModel>(source.Location)
                };
            return ret;
        }
    }

    public class VenueBoardViewModel : BoardViewModel
    {
        public List<VenueInformationModel> VenueInformation { get; set; }
        public LocationModel Location { get; set; }
    }
}