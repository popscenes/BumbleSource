using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MbUnit.Framework;
using Ninject;
using Ninject.MockingKernel.Moq;
using PostaFlya.Domain.Comments;
using WebSite.Infrastructure.Command;
using WebSite.Infrastructure.Query;

namespace PostaFlya.Mocks.Domain.Data
{
    public static class CommentTestData
    {
        public static bool AssertStoreRetrieve(CommentInterface storedComment, CommentInterface retrievedComment)
        {
            Assert.AreEqual(storedComment.Id, retrievedComment.Id);
            Assert.AreEqual(storedComment.AggregateId, retrievedComment.AggregateId);
            Assert.AreEqual(storedComment.BrowserId, retrievedComment.BrowserId);
            Assert.AreEqual(storedComment.CommentContent, retrievedComment.CommentContent);
            Assert.AreApproximatelyEqual(storedComment.CommentTime, retrievedComment.CommentTime, TimeSpan.FromMilliseconds(1));
            return true;
        }

        public static bool Equals(CommentInterface storedComment, CommentInterface retrievedComment)
        {
            return storedComment.Id == retrievedComment.Id &&
                   storedComment.AggregateId == retrievedComment.AggregateId &&
                   storedComment.BrowserId == retrievedComment.BrowserId &&
                   storedComment.CommentContent == retrievedComment.CommentContent &&
                   storedComment.CommentTime - retrievedComment.CommentTime < TimeSpan.FromMilliseconds(1);
        }

        internal static CommentInterface AssertGetById(CommentInterface comment, GenericQueryServiceInterface queryService)
        {
            var retrievedFlier = queryService.FindById<Comment>(comment.Id);
            AssertStoreRetrieve(comment, retrievedFlier);

            return retrievedFlier;
        }


        internal static CommentInterface StoreOne(CommentInterface comment, GenericRepositoryInterface repository, StandardKernel kernel)
        {
            var uow = kernel.Get<UnitOfWorkFactoryInterface>()
                .GetUnitOfWork(new List<RepositoryInterface>() { repository });
            using (uow)
            {

                repository.Store(comment);
            }

            Assert.IsTrue(uow.Successful);
            return comment;
        }

        internal static IList<CommentInterface> StoreSome(GenericRepositoryInterface repository, StandardKernel kernel, string entityId)
        {
            var ret = GetSome(kernel, entityId);
            using (kernel.Get<UnitOfWorkFactoryInterface>()
                .GetUnitOfWork(new List<RepositoryInterface>() { repository }))
            {
                foreach (var comment in ret)
                {
                    repository.Store(comment);
                }
            }
            return ret;
        }

        internal static IList<CommentInterface> GetSome(StandardKernel kernel, string entityId, int num  = 5)
        {
            var ret = new List<CommentInterface>();
            var time = DateTime.UtcNow;
            for (int i = 0; i < num; i++)
            {
                var comment = GetOne(kernel, entityId);
                comment.CommentTime = time;
                comment.CommentContent = "Comment number " + i;
                time = time.AddMinutes(10);
                ret.Add(comment);
            }
            return ret;
        }

        internal static void UpdateOne(CommentInterface comment, GenericRepositoryInterface repository, StandardKernel kernel)
        {
            using (kernel.Get<UnitOfWorkFactoryInterface>()
                .GetUnitOfWork(new List<RepositoryInterface>() { repository }))
            {
                repository.UpdateEntity<Comment>(comment.Id, e => e.CopyFieldsFrom(comment));
            }
        }

        public static CommentInterface GetOne(StandardKernel kernel, string entityId)
        {

//            var dattimedesc = (DateTime.MaxValue.Ticks - DateTime.UtcNow.Ticks).ToString("D20");
//            var dattimeasc = (DateTime.UtcNow.Ticks).ToString("D20");
            
            var ret = new Comment
                          {
                              Id = Guid.NewGuid().ToString(),
                              AggregateId = entityId,
                              BrowserId = Guid.NewGuid().ToString(),
                              CommentContent = "This is a comment",
                              CommentTime = DateTime.UtcNow
                          };
            return ret;
        }
    }
}
