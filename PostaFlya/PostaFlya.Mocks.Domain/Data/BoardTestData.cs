using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using PostaFlya.Domain.Boards;

namespace PostaFlya.Mocks.Domain.Data
{
    public static class BoardTestData
    {

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
