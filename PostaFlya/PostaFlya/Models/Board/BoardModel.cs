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
using Website.Infrastructure.Query;

namespace PostaFlya.Models.Board
{
    public class GetBoardModelsByIdsQueryHandler : QueryHandlerInterface<GetBoardsByIdsQuery, List<BoardModel>>
    {
        private readonly QueryChannelInterface _queryChannel;

        public GetBoardModelsByIdsQueryHandler(QueryChannelInterface queryChannel)
        {
            _queryChannel = queryChannel;
        }

        public List<BoardModel> Query(GetBoardsByIdsQuery argument)
        {
            var ret = _queryChannel.Query(argument, new List<PostaFlya.Domain.Boards.Board>());
            return _queryChannel.ToViewModel<BoardModel, PostaFlya.Domain.Boards.Board>(ret);
        }
    }

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
            target.VenueInformation = source
                .InformationSources
                .Select(information => _queryChannel.ToViewModel<VenueInformationModel>(information))
                .ToList();
            target.Location = _queryChannel.ToViewModel<LocationModel>(source.InformationSources.First().Address);
            target.BoardTypeEnum = source.BoardTypeEnum;
            target.Id = source.Id;
            target.DefaultVenueInformation =
                target.VenueInformation.FirstOrDefault(model => model.Source == source.DefaultInformationSource);
            target.DefaultVenueInformation = target.DefaultVenueInformation ?? target.VenueInformation.FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(source.ImageId))
                target.BoardImageUrl = _blobStorage.GetBlobUri(source.ImageId).ToString();
            else if (source.InformationSources.First().Address != null && source.InformationSources.First().Address.IsValid)
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
        private readonly BlobStorageInterface _blobStorage;

        public ToBoardSummaryModel(QueryChannelInterface queryChannel, [ImageStorage]BlobStorageInterface blobStorage)
        {
            _queryChannel = queryChannel;
            _blobStorage = blobStorage;
        }

        public BoardSummaryModel ToViewModel(BoardSummaryModel target, Domain.Boards.Board source)
        {
            if (target == null)
                target = new BoardSummaryModel();
            target.Name = source.Name;
            target.Description = source.Description;
            target.Location = _queryChannel.ToViewModel<VenueInformationModel>(source.InformationSources.First());
            target.Id = source.Id;
            return target;
        }

    }

    [DataContract]
    public class BoardSummaryModel
    {
        public BoardSummaryModel()
        {
            Location = new VenueInformationModel();
        }
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public VenueInformationModel Location { get; set; }

    }

    [DataContract]
    public class BoardModel
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
}