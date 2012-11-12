using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Ninject;
using PostaFlya.Domain.Boards;
using PostaFlya.Domain.Boards.Query;
using Website.Domain.Location;
using Website.Infrastructure.Command;

namespace PostaFlya.Mocks.Domain.Data
{
    public static class BoardTestData
    {

        public static Board GetOne(IKernel kernel, String boardName, Location loc = null)
        {

            var ret = new Board()
            {
                Id = Guid.NewGuid().ToString(),
                FriendlyId = boardName,
                Location = loc,
                BrowserId = kernel.Get<string>(bm => bm.Has("defaultbrowserid")),
                RequireApprovalOfPostedFliers = true,
                AllowOthersToPostFliers = true,
            };
            ret.FriendlyId = BoardQueryServiceUtil.FindFreeFriendlyId(null, ret);
            return ret;
        }

        internal static Board StoreOne(Board board, GenericRepositoryInterface repository, StandardKernel kernel)
        {
            var uow = kernel.Get<UnitOfWorkFactoryInterface>()
                .GetUnitOfWork(new List<RepositoryInterface>() { repository });
            using (uow)
            {

                repository.Store(board);
            }

            Assert.IsTrue(uow.Successful);
            return board;
        }

        internal static BoardFlier StoreOne(BoardFlier boardFlier, GenericRepositoryInterface repository, StandardKernel kernel)
        {
            var uow = kernel.Get<UnitOfWorkFactoryInterface>()
                .GetUnitOfWork(new List<RepositoryInterface>() { repository });
            using (uow)
            {

                repository.Store(boardFlier);
            }

            Assert.IsTrue(uow.Successful);
            return boardFlier;
        }


        public static void AssertStoreRetrieve(BoardInterface storedBoard, BoardInterface retrievedBoard)
        {
            Assert.AreEqual(storedBoard.Id, retrievedBoard.Id);
            Assert.AreEqual(storedBoard.FriendlyId, retrievedBoard.FriendlyId);
            Assert.AreEqual(storedBoard.BrowserId, retrievedBoard.BrowserId);
            Assert.AreEqual(storedBoard.Location, retrievedBoard.Location);
            Assert.AreEqual(storedBoard.RequireApprovalOfPostedFliers, retrievedBoard.RequireApprovalOfPostedFliers);
            Assert.AreEqual(storedBoard.AllowOthersToPostFliers, retrievedBoard.AllowOthersToPostFliers);
        }
    }
}
