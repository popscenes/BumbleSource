using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Ninject;
using Website.Infrastructure.Command;
using Website.Infrastructure.Query;
using Website.Domain.Comments;
using Website.Test.Common;

namespace Website.Mocks.Domain.Data
{
    public static class CommentTestData
    {
        public static bool AssertStoreRetrieve(CommentInterface storedComment, CommentInterface retrievedComment)
        {
            Assert.AreEqual(storedComment.Id, retrievedComment.Id);
            Assert.AreEqual(storedComment.AggregateId, retrievedComment.AggregateId);
            Assert.AreEqual(storedComment.BrowserId, retrievedComment.BrowserId);
            Assert.AreEqual(storedComment.CommentContent, retrievedComment.CommentContent);
            AssertUtil.AreEqual(storedComment.CommentTime, retrievedComment.CommentTime, TimeSpan.FromMilliseconds(1));
            return true;
        }

        public class CommentTestDataEq : IComparer
        {
            public int Compare(object x, object y)
            {
                return CommentTestData.Equals((CommentInterface)x, (CommentInterface)y) ? 0 : 1;
            }
        }

        public static bool Equals(CommentInterface storedComment, CommentInterface retrievedComment)
        {
            return storedComment.Id == retrievedComment.Id &&
                   storedComment.AggregateId == retrievedComment.AggregateId &&
                   storedComment.BrowserId == retrievedComment.BrowserId &&
                   storedComment.CommentContent == retrievedComment.CommentContent &&
                   storedComment.CommentTime - retrievedComment.CommentTime < TimeSpan.FromMilliseconds(1);
        }

//        internal static CommentInterface AssertGetById(CommentInterface comment, GenericQueryServiceInterface queryService)
//        {
//            var retrievedFlier = queryService.FindByAggregate<Comment>(comment.Id, comment.AggregateId);
//            AssertStoreRetrieve(comment, retrievedFlier);
//
//            return retrievedFlier;
//        }

//
//        internal static CommentInterface StoreOne(CommentInterface comment, GenericRepositoryInterface repository, StandardKernel kernel)
//        {
//            var uow = kernel.Get<UnitOfWorkInterface>().Begin();
//            using (uow)
//            {
//
//                repository.Store(comment);
//            }
//
//            Assert.IsTrue(uow.Successful);
//            return comment;
//        }

//        internal static IList<CommentInterface> StoreSome(GenericRepositoryInterface repository, StandardKernel kernel, string entityId)
//        {
//            var ret = GetSome(kernel, entityId);
//            using (kernel.Get<UnitOfWorkInterface>().Begin())
//            {
//                foreach (var comment in ret)
//                {
//                    repository.Store(comment);
//                }
//            }
//            return ret;
//        }

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
                comment.SetId();
                ret.Add(comment);
            }
            return ret;
        }

//        internal static void UpdateOne(CommentInterface comment, GenericRepositoryInterface repository, StandardKernel kernel)
//        {
//            using (kernel.Get<UnitOfWorkInterface>().Begin())
//            {
//                repository.UpdateAggregateEntity<Comment>(comment.Id, comment.AggregateId, e => e.CopyFieldsFrom(comment));
//            }
//        }

        public static CommentInterface GetOne(StandardKernel kernel, string entityId)
        {            
            var ret = new Comment
                          {
                              AggregateId = entityId,
                              BrowserId = Guid.NewGuid().ToString(),
                              CommentContent = "This is a comment",
                              CommentTime = DateTime.UtcNow
                          };
            ret.SetId();
            return ret;
        }
    }
}
