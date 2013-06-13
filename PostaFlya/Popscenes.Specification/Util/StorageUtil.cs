using System;
using System.Collections.Generic;
using System.Data.Services.Client;
using System.Linq;
using Ninject;
using PostaFlya.DataRepository.Search.Implementation;
using PostaFlya.Domain.Flier;
using PostaFlya.Mocks.Domain.Data;
using TechTalk.SpecFlow;
using Website.Application.Command;
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

        public static void StoreAll(IList<Flier> fliers)
        {
            foreach (var flier in fliers)
            {
                Store(flier);
            }
        }

        public static void Store(Flier flier)
        {
            var repo = SpecUtil.Kernel.Get<GenericRepositoryInterface>();
            FlierTestData.StoreOnePublishEvent(flier, repo, SpecUtil.Kernel);
        }




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


        public static void InitTableStorage()
        {
            var tableNameProv = SpecUtil.Kernel.Get<TableNameAndIndexProviderServiceInterface>();

            tableNameProv.SuffixTableNames("spec");

            SpecUtil.Kernel.Get<SqlSeachDbInitializer>().DeleteAll();

            AzureEnv.UseRealStorage = false;
            var tctx = SpecUtil.Kernel.Get<TableContextInterface>();
            foreach (var tableName in tableNameProv.GetAllTableNames())
            {
                tctx.InitTable<JsonTableEntry>(tableName);
                tctx.Delete<JsonTableEntry>(tableName, null);
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
            var processor = SpecUtil.Kernel.Get<QueuedCommandProcessor>(ctx => ctx.Has("workercommandqueue"));
            QueuedCommandProcessor.WorkInProgress wip = null;
            while ((wip = processor.ProcessOneSynch()) != null)
            {
                ScenarioSentWorkerBusMessagesForContext.Add(wip);
            }
        }
    }
}
