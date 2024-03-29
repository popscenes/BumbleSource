﻿using System;
using System.Collections.Generic;
using System.Data.Services.Client;
using System.Linq;
using NUnit.Framework;
using Ninject;
using PostaFlya.DataRepository.Search.Implementation;
using PostaFlya.Domain.Boards;
using PostaFlya.Domain.Flier;
using PostaFlya.Mocks.Domain.Data;
using TechTalk.SpecFlow;
using Website.Application.Messaging;
using Website.Azure.Common.Environment;
using Website.Azure.Common.TableStorage;
using Website.Infrastructure.Command;
using Website.Infrastructure.Domain;
using Website.Infrastructure.Query;

namespace Popscenes.Specification.Util
{
    public static class StorageUtil
    {

        //Todo move domain events to repo and make all this generic
//        public static void Store<EntityType>(EntityType store)
//        {
//            PerformInUow((session) => session.Store(store));
//        }
//
//        public static void StoreAll<EntityType>(IEnumerable<EntityType> store)
//        {
//            PerformInUow((session) =>
//                {
//                    foreach (var entity in store)
//                    {
//                        session.Store(entity);
//                    }
//                });
//        }
//
//        public static void Update<EntityType>(string id, Action<EntityType> update) where EntityType : class, AggregateRootInterface, new()
//        {
//            PerformInUow((session) => session.UpdateEntity(id, update));
//        }

//        public static void StoreAll(IList<Flier> fliers)
//        {
//            foreach (var flier in fliers)
//            {
//                Store(flier);
//            }
//        }

        public static void Store<EntType>(EntType flier)
        {
            var repository = SpecUtil.Kernel.Get<GenericRepositoryInterface>();
            var uow = SpecUtil.Kernel.Get<UnitOfWorkFactoryInterface>()
                .GetUnitOfWork(new List<RepositoryInterface>() { repository });
            using (uow)
            {

                repository.Store(flier);
            }

            Assert.IsTrue(uow.Successful);
        }

        public static void StoreAll<EntType>(IList<EntType> entities)
        {
            foreach (var board in entities)
            {
                Store(board);
            }
        }

//        public static void Store(Board board)
//        {
//            var repo = SpecUtil.Kernel.Get<GenericRepositoryInterface>();
//            BoardTestData.StoreOne(board, repo, SpecUtil.Kernel);
//        }


        public static EntityType Get<EntityType>(string id) where EntityType : class, AggregateRootInterface, new()
        {
            var qs = SpecUtil.Kernel.Get<GenericQueryServiceInterface>();
            return qs.FindById<EntityType>(id);
        }

        public static IList<EntityType> Get<EntityType>(IList<string> ids) where EntityType : class, AggregateRootInterface, new()
        {
            var qs = SpecUtil.Kernel.Get<GenericQueryServiceInterface>();
            return qs.FindByIds<EntityType>(ids).ToList();
        }

        public static void PerformInUow(Action<GenericRepositoryInterface> action)
        {
            var repo = SpecUtil.Kernel.Get<GenericRepositoryInterface>();
            using (var uow = SpecUtil.Kernel.Get<UnitOfWorkFactoryInterface>().GetUnitOfWork(repo))
            {
                action(repo);
            }
        }

        private static bool _runonce = false; 
        public static void InitTableStorage()
        {
            var tableNameProv = SpecUtil.Kernel.Get<TableNameAndIndexProviderServiceInterface>();

            tableNameProv.SuffixTableNames("spec");

            if (!_runonce)
            {
                CreateTableStorage();
                _runonce = true;
            }

            SpecUtil.Kernel.Get<SqlSeachDbInitializer>().DeleteAll();

            AzureEnv.UseRealStorage = false;
            var tctx = SpecUtil.Kernel.Get<TableContextInterface>();
            foreach (var tableName in tableNameProv.GetAllTableNames())
            {
                tctx.Delete<JsonTableEntry>(tableName, null);
            }
            tctx.SaveChanges(SaveChangesOptions.ReplaceOnUpdate);
        }

        public static void CreateTableStorage()
        {
            var tableNameProv = SpecUtil.Kernel.Get<TableNameAndIndexProviderServiceInterface>();

            SpecUtil.Kernel.Get<SqlSeachDbInitializer>().Initialize();

            AzureEnv.UseRealStorage = false;
            var tctx = SpecUtil.Kernel.Get<TableContextInterface>();
            foreach (var tableName in tableNameProv.GetAllTableNames())
            {
                tctx.InitTable<JsonTableEntry>(tableName);
            }
            tctx.SaveChanges(SaveChangesOptions.ReplaceOnUpdate);
        }

        private const string SentWorkerBusMessagesForContext = "SentBusMessagesForContext";
        public static List<object> ScenarioSentWorkerBusMessagesForContext
        {
            get
            {
                var ret = ScenarioContext.Current.TryGet<List<object>>(SentWorkerBusMessagesForContext);
                if (ret == null)
                {
                    ret = new List<object>();
                    ScenarioContext.Current[SentWorkerBusMessagesForContext] = ret;
                }
                return ret;
            }
        }

        public static void ProcessAllMessagesAndEvents()
        {
            var processor = SpecUtil.Kernel.Get<QueuedMessageProcessor>(ctx => ctx.Has("workercommandqueue"));
            QueuedMessageProcessor.WorkInProgress wip = null;
            while ((wip = processor.ProcessOneSynch()) != null)
            {
                ScenarioSentWorkerBusMessagesForContext.Add(wip);
            }
        }
    }
}
