using System;
using System.Linq;
using System.Text;
using Ninject;
using WebSite.Azure.Common.TableStorage;
using PostaFlya.DataRepository.Flier;
using PostaFlya.Domain.Comments;
using PostaFlya.Domain.Flier;
using WebSite.Infrastructure.Query;

namespace PostaFlya.DataRepository.Internal
{

    internal class AzureCommentRepository : 
        AzureRepositoryBase<CommentInterface, CommentStorageDomain>, 
        GenericQueryServiceInterface<CommentInterface>
    {
        private readonly AzureTableContext _tableContext;
        public AzureCommentRepository([Named("comments")]AzureTableContext tableContext) : base(tableContext)
        {
            _tableContext = tableContext;
        }

        protected override CommentStorageDomain GetEntityForUpdate(string id)
        {
            return CommentStorageDomain.GetEntityForUpdate(id, _tableContext);
        }

        protected override CommentStorageDomain GetStorageForEntity(CommentInterface entity)
        {
            return new CommentStorageDomain(entity, _tableContext);
        }

        public CommentInterface FindById(string id)
        {
            return CommentStorageDomain.FindById(id, _tableContext);
        }

        object QueryServiceInterface.FindById(string id)
        {
            return FindById(id);
        }

        public IQueryable<CommentInterface> GetByEntity(string entityId, int take = -1)
        {
            return CommentStorageDomain.FindRelatedEntities(entityId, _tableContext, take);
        }
    }
}
