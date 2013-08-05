using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using PostaFlya.Domain.Boards;
using PostaFlya.Domain.Boards.Query;
using PostaFlya.Domain.Venue;
using PostaFlya.Models.Location;
using Website.Application.Binding;
using Website.Application.Content;
using Website.Application.Domain.Google.Places;
using Website.Application.Google.Content;
using Website.Common.Model;
using Website.Common.Model.Query;
using Website.Domain.Location;
using Website.Infrastructure.Query;
using Loc = Website.Domain.Location;

namespace PostaFlya.Models.Board
{
    public class ToBoardModel
    : ViewModelMapperInterface<BoardModel, PostaFlya.Domain.Boards.Board>
    {
        private readonly QueryChannelInterface _queryChannel;
        private readonly BlobStorageInterface _blobStorage;

        public ToBoardModel(QueryChannelInterface queryChannel, [ImageStorage]BlobStorageInterface blobStorage)
        {
            _queryChannel = queryChannel;
            _blobStorage = blobStorage;
        }

        public BoardModel ToViewModel(BoardModel target, Domain.Boards.Board source)
        {
            if (target == null)
                target = new BoardModel();
            target.Name = source.Name;
            target.FriendlyId = source.FriendlyId;
            target.Description = source.Description;
            target.VenueInformation = source.InformationSources == null ? new List<VenueInformationModel>() :
                source.InformationSources
                .Select(information => _queryChannel.ToViewModel<VenueInformationModel, VenueInformation>(information))
                .ToList();
            target.Location = target.Venue() == null ? null : target.Venue().Address;
            target.BoardTypeEnum = source.BoardTypeEnum;
            target.Id = source.Id;
            target.DefaultVenueInformation = target.Venue();
            if (!string.IsNullOrWhiteSpace(source.ImageId))
                target.BoardImageUrl = _blobStorage.GetBlobUri(source.ImageId).ToString();
            else if (target.DefaultVenueInformation != null && target.DefaultVenueInformation.Address != null && target.DefaultVenueInformation.Address.IsValid())
            {
                target.BoardImageUrl = source.InformationSources.First().Address.GoogleMapsUrl(400, 200);
                target.BoardImageExternal = true;
            }

            return target;
        }

    }

    public class ToBoardSummaryModel
    : ViewModelMapperInterface<BoardSummaryModel, PostaFlya.Domain.Boards.Board>
    {
        private readonly QueryChannelInterface _queryChannel;

        public ToBoardSummaryModel(QueryChannelInterface queryChannel)
        {
            _queryChannel = queryChannel;
        }

        public BoardSummaryModel ToViewModel(BoardSummaryModel target, Domain.Boards.Board source)
        {
            if (target == null)
                target = new BoardSummaryModel();
            target.Name = source.Name;
            target.Description = source.Description;
            target.Location = _queryChannel.ToViewModel<VenueInformationModel, VenueInformation>(source.InformationSources.First());
            target.Id = source.Id;
            target.FriendlyId = source.FriendlyId;
            return target;
        }

    }

    [DataContract]
    public class BoardSummaryModel : IsModelInterface
    {
        public BoardSummaryModel()
        {
            Location = new VenueInformationModel(){Address = new LocationModel()};
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
        public VenueInformationModel Location { get; set; }

    }

    [DataContract]
    public class BoardModel : IsModelInterface
    {
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

        [DataMember]
        public bool BoardImageExternal { get; set; }
        
        [DataMember]
        public string BoardImageUrl { get; set; }

    }

    public static class BoardModelExtensions
    {

        public static VenueInformationModel Venue(this BoardModel board)
        {
            if (board.VenueInformation == null || board.VenueInformation.Count == 0)
                return null;

            if (board.DefaultVenueInformation != null)
            {
                var ret =
                    board.VenueInformation.FirstOrDefault(
                        information => information.Source == board.DefaultVenueInformation.Source);
                return ret ?? board.VenueInformation.First();
            }

            return board.VenueInformation.First();
        }
    }
}