//using System;
//using System.Linq;
//using System.Text;
//using Ninject;
//using WebSite.Azure.Common.TableStorage;
//using PostaFlya.DataRepository.Flier;
//using PostaFlya.Domain.Comments;
//using PostaFlya.Domain.Flier;
//using WebSite.Infrastructure.Query;
//
//namespace PostaFlya.DataRepository.Internal
//{
//
//    internal class AzureCommentRepository : JsonRepository
//    {
//        public AzureCommentRepository(TableContextInterface tableContext
//            , TableNameAndPartitionProviderServiceInterface nameAndPartitionProviderService) 
//            : base(tableContext, nameAndPartitionProviderService)
//        {
//        }
//
//        public IQueryable<CommentInterface> GetByEntity(string entityId, int take = -1)
//        {
//            return FindAggregateEntities<Comment>(entityId, take);
//        }
//
//    }
//}
