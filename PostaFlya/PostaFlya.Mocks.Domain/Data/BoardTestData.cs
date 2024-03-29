﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Ninject;
using PostaFlya.Domain.Boards;
using PostaFlya.Domain.Boards.Query;
using PostaFlya.Domain.Venue;
using Website.Domain.Location;
using Website.Infrastructure.Command;
using Website.Infrastructure.Domain;
using Website.Infrastructure.Messaging;
using Website.Infrastructure.Publish;

namespace PostaFlya.Mocks.Domain.Data
{
    public static class BoardTestData
    {

        public static Board GetAndStoreOne(IKernel kernel, GenericRepositoryInterface repository, String boardName = "TestBoard",
                                    BoardTypeEnum typeOfBoard = BoardTypeEnum.VenueBoard, Location loc = null)
        {
            var board = GetOne(kernel, boardName, typeOfBoard, loc);
            return StoreOne(board, repository, kernel);
        }
        public static Board GetOne(IKernel kernel, String boardName = "TestBoard", BoardTypeEnum typeOfBoard = BoardTypeEnum.VenueBoard, Location loc = null)
        {
            var venuInfo = new VenueInformation();
            venuInfo.Address = loc ?? kernel.Get<Location>(ib => ib.Get<bool>("default"));

            var ret = typeOfBoard != BoardTypeEnum.InterestBoard ? new Board() { InformationSources = new List<VenueInformation>(){venuInfo}} : new Board();
            ret.Id = Guid.NewGuid().ToString();
            ret.FriendlyId = boardName;
            ret.BrowserId = kernel.Get<string>(bm => bm.Has("defaultbrowserid"));
            ret.RequireApprovalOfPostedFliers = true;
            ret.AllowOthersToPostFliers = true;
            ret.BoardTypeEnum = typeOfBoard;

            ret.FriendlyId = BoardQueryServiceUtil.FindFreeFriendlyId(null, ret);
            return ret;
        }

        internal static Board StoreOne(Board board, GenericRepositoryInterface repository, IKernel kernel)
        {
            var uow = kernel.Get<UnitOfWorkFactoryInterface>()
                .GetUnitOfWork(new List<RepositoryInterface>() { repository });
            using (uow)
            {

                repository.Store(board);
            }

            Assert.IsTrue(uow.Successful);

            /*if (uow.Successful)
            {
                var indexers = kernel.GetAll<HandleEventInterface<EntityModifiedEvent<Board>>>();
                foreach (var handleEvent in indexers)
                {
                    handleEvent.Handle(new BoardModifiedEvent() { Entity = board });
                }
            }*/

            return board;
        }

        internal static void UpdateOne(Board board, GenericRepositoryInterface repository, StandardKernel kernel)
        {
            Board oldState = null;
            UnitOfWorkInterface unitOfWork;
            using (unitOfWork = kernel.Get<UnitOfWorkFactoryInterface>().GetUnitOfWork(new List<RepositoryInterface>() { repository }))
            {
                repository.UpdateEntity<Board>(board.Id, e =>
                {
                    oldState = e.CreateCopy<Board, Board>(BoardInterfaceExtensions.CopyFieldsFrom);
                    e.CopyFieldsFrom(board);
                });
            }

//            if (unitOfWork.Successful)
//            {
//                var indexers = kernel.GetAll<HandleEventInterface<EntityModifiedEvent<Board>>>();
//                foreach (var handleEvent in indexers)
//                {
//                    handleEvent.Handle(new BoardModifiedEvent() { Entity = board });
//                }
//            }
        }


        public static void AssertStoreRetrieve(BoardInterface storedBoard, BoardInterface retrievedBoard)
        {
            Assert.AreEqual(storedBoard.Id, retrievedBoard.Id);
            Assert.AreEqual(storedBoard.FriendlyId, retrievedBoard.FriendlyId);
            Assert.AreEqual(storedBoard.BrowserId, retrievedBoard.BrowserId);
            Assert.AreEqual(storedBoard.RequireApprovalOfPostedFliers, retrievedBoard.RequireApprovalOfPostedFliers);
            Assert.AreEqual(storedBoard.AllowOthersToPostFliers, retrievedBoard.AllowOthersToPostFliers);
            if(storedBoard.BoardTypeEnum != BoardTypeEnum.InterestBoard)
                Assert.AreEqual(storedBoard.InformationSources.First().Address, retrievedBoard.InformationSources.First().Address);
        }
    }
}
