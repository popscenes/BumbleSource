using System;
using System.Collections.Generic;
using System.Data.Services.Client;
using System.Linq;
using Ninject;
using WebSite.Azure.Common.TableStorage;
using PostaFlya.DataRepository.Internal;
using PostaFlya.DataRepository.Search.Services;
using PostaFlya.Domain.Behaviour;
using PostaFlya.Domain.Browser;
using PostaFlya.Domain.Comments;
using PostaFlya.Domain.Flier;
using PostaFlya.Domain.Flier.Command;
using PostaFlya.Domain.Flier.Query;
using PostaFlya.Domain.Likes;
using PostaFlya.Domain.Location;
using PostaFlya.Domain.Tag;
using WebSite.Infrastructure.Command;
using WebSite.Infrastructure.Query;

namespace PostaFlya.DataRepository.Flier
{
    internal class AzureFlierRepository : AzureRepositoryBase<FlierInterface, FlierStorageDomain>
        , FlierRepositoryInterface
        , FlierQueryServiceInterface
    {
        private readonly AzureTableContext _tableContext;
        private readonly LocationServiceInterface _locationService;
        private readonly AzureCommentRepository _commentRepository;
        private readonly AzureLikeRepository _likeRepository;
        private readonly FlierSearchServiceInterface _flierSearchService;

        public AzureFlierRepository(
            [Named("flier")]AzureTableContext flierTableContext
            , AzureCommentRepository commentRepository
            , AzureLikeRepository likeRepository
            , FlierSearchServiceInterface flierSearchService
            )
            : base(flierTableContext, flierSearchService)
        {
            _tableContext = flierTableContext;
            _commentRepository = commentRepository;
            _likeRepository = likeRepository;
            _flierSearchService = flierSearchService;
        }

        #region Implementation of FlierQueryServiceInterface

        public IList<string> FindFliersByLocationTagsAndDistance(Location location, Tags tags, int distance = 0, int take = 0, FlierSortOrder sortOrder = FlierSortOrder.CreatedDate, int skip = 0)
        {
            return _flierSearchService
                .FindFliersByLocationTagsAndDistance(location, tags, distance, take, sortOrder,
                                                                           skip);
            //BoundingBox boundingBox = _locationService.GetBoundingBox(location, distance);

            //return FindFliersByLocationTagsAndBoundingBox(location, tags, boundingBox, take, sortOrder, endOfPageFlier);

        }

        public FlierInterface FindById(string id)
        {
            return FlierStorageDomain.FindById(id, _tableContext);
        }

        public IQueryable<FlierInterface> GetByBrowserId(String browserId)
        {
            if (string.IsNullOrWhiteSpace(browserId))
                return null;

            return FlierStorageDomain.GetByBrowserId(browserId, _tableContext);
        }

        #endregion

        protected IList<string> FindFliersByLocationTagsAndBoundingBox(Location location
            , Tags tags, BoundingBox boundingBox, int take, FlierSortOrder sortOrder, FlierInterface endOfPageFlier)
        {
//            return FlierStorageDomain.FindFliersByLocationTagsAndBoundingBox(location, tags, boundingBox,
//                                                                            take, _tableContext);

            

//            return FlierStorageDomain.FindSortedFliersByLocationTagsAndBoundingBox(location, tags, boundingBox,
//                                                                take, sortOrder, endOfPageFlier, _tableContext);
            return null;
        }

        protected override FlierStorageDomain GetEntityForUpdate(string id)
        {
            return FlierStorageDomain.GetEntityForUpdate(id, _tableContext);
        }

        protected override FlierStorageDomain GetStorageForEntity(FlierInterface entity)
        {
            return new FlierStorageDomain(entity, _tableContext);
        }

        public CommentableInterface AddComment(CommentInterface comment)
        {
            var queryFlier = FindById(comment.EntityId);
            if (queryFlier == null)
                return null;

            if(!_commentRepository.PerformActionInSingleUnitOfWork(r => r.Store(comment)))
                return queryFlier;

            UpdateEntity(comment.EntityId, flier => { 
                flier.NumberOfComments++;
                queryFlier.CopyFieldsFrom(flier);
            });

            return queryFlier;            
        }

        public IQueryable<CommentInterface> GetComments(string flierId, int take = -1)
        {
            return _commentRepository.GetByEntity(flierId, take);
        }

        object QueryServiceInterface.FindById(string id)
        {
            return FindById(id);
        }

        public LikeableInterface Like(LikeInterface like)
        {
            var queryFlier = FindById(like.EntityId);
            if (queryFlier == null)
                return null;

            like.EntityTypeTag = "Flier";

            var idKey = LikeStorageDomain.GetIdPartitionKey(like);
            var existingLike = _likeRepository.FindById(idKey);
            if(existingLike == null)
            {
                if(!_likeRepository.PerformActionInSingleUnitOfWork(l => l.Store(like)))
                    return null;

                //TODO move to background command if needed
                UpdateEntity(like.EntityId, flier =>
                {
                    flier.NumberOfLikes =
                        _likeRepository.GetByEntity(like.EntityId).Count();
                    queryFlier.CopyFieldsFrom(flier);
                });                
            }

            return queryFlier;
        }

        public IQueryable<LikeInterface> GetLikes(string id)
        {
            return _likeRepository.GetByEntity(id);
        }

        public IQueryable<LikeInterface> GetLikesByBrowser(string browserId)
        {
            return _likeRepository.FindByBrowserAndEntityTypeTag(browserId, "Flier");
        }
    }
}