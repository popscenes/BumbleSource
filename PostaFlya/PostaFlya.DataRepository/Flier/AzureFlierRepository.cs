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
    internal class AzureFlierRepository : JsonRepositoryWithBrowser
        , FlierRepositoryInterface
        , FlierQueryServiceInterface
    {
        //private readonly AzureTableContext _tableContext;
        private readonly LocationServiceInterface _locationService;
//        private readonly AzureCommentRepository _commentRepository;
//        private readonly AzureLikeRepository _likeRepository;
        private readonly FlierSearchServiceInterface _flierSearchService;


        #region Implementation of FlierQueryServiceInterface

        public AzureFlierRepository(TableContextInterface tableContext
            , TableNameAndPartitionProviderServiceInterface nameAndPartitionProviderService
            , LocationServiceInterface locationService
//            , AzureCommentRepository commentRepository
//            , AzureLikeRepository likeRepository
            , FlierSearchServiceInterface flierSearchService)
            : base(tableContext, nameAndPartitionProviderService, flierSearchService)
        {
            _locationService = locationService;
//            _commentRepository = commentRepository;
//            _likeRepository = likeRepository;
            _flierSearchService = flierSearchService;
        }

        public IList<string> FindFliersByLocationTagsAndDistance(Location location, Tags tags, int distance = 0, int take = 0, FlierSortOrder sortOrder = FlierSortOrder.CreatedDate, int skip = 0)
        {
            return _flierSearchService
                .FindFliersByLocationTagsAndDistance(location, tags, distance, take, sortOrder,
                                                                           skip);
       
        }


        #endregion

//        public CommentableInterface AddComment(CommentInterface comment)
//        {
//            var queryFlier = FindById<Domain.Flier.Flier>(comment.EntityId);
//            if (queryFlier == null)
//                return null;
//
//            if(!_commentRepository.PerformActionInSingleUnitOfWork(r => r.Store(comment)))
//                return queryFlier;
//
//            UpdateEntity<Domain.Flier.Flier>(comment.EntityId, flier => { 
//                flier.NumberOfComments++;
//                queryFlier.CopyFieldsFrom(flier);
//            });
//
//            return queryFlier;            
//        }
//
//        public IQueryable<CommentInterface> GetComments(string flierId, int take = -1)
//        {
//            return _commentRepository.GetByEntity(flierId, take);
//        }
//
//        public LikeableInterface Like(LikeInterface like)
//        {
//            var queryFlier = FindById<Domain.Flier.Flier>(like.EntityId);
//            if (queryFlier == null)
//                return null;
//
//            like.EntityTypeTag = "Flier";
//
//            var idKey = AzureLikeRepository.GetIdPartitionKey(like);
//            
//            var existingLike = _likeRepository.FindById<Like>(idKey);
//            if(existingLike == null)
//            {
//                if(!_likeRepository.PerformActionInSingleUnitOfWork(l => l.Store(like)))
//                    return null;
//
//                //TODO move to background command if needed
//                UpdateEntity<Domain.Flier.Flier>(like.EntityId, flier =>
//                {
//                    flier.NumberOfLikes =
//                        _likeRepository.GetByEntity(like.EntityId).Count();
//                    queryFlier.CopyFieldsFrom(flier);
//                });                
//            }
//
//            return queryFlier;
//        }
//
//        public IQueryable<LikeInterface> GetLikes(string id)
//        {
//            return _likeRepository.GetByEntity(id);
//        }
//
//        public IQueryable<LikeInterface> GetLikesByBrowser(string browserId)
//        {
//            return _likeRepository.FindByBrowserAndEntityTypeTag(browserId, "Flier");
//        }
    }
}