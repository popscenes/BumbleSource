using System;
using System.Collections.Generic;
using System.Data.Services.Client;
using Ninject;
using PostaFlya.DataRepository.Search.Implementation;
using TechTalk.SpecFlow;
using Website.Application.Command;
using Website.Azure.Common.Environment;
using Website.Azure.Common.TableStorage;

namespace Popscenes.Specification.Util
{
    public static class StorageUtil
    {
        public static void Store<TEntity>(TEntity store)
        {
            PerformInUow((session) =>
            {

            });
        }

        public static void StoreAll(IEnumerable<dynamic> storeAll)
        {
            PerformInUow((session) =>
            {
                foreach (var o in storeAll)
                {

                }

            });
        }

        public static TEntity Get<TEntity>(string id) where TEntity : class
        {
            TEntity ret = null;
            PerformInUow((session) =>
            {

            });
            return ret;
        }

        public static IList<TEntity> Get<TEntity>(IList<string> ids)
        {
            IList<TEntity> ret = null;
            PerformInUow((session) =>
            {

            });

            return ret;
        }




        public static void PerformInUow(Action<TableContext> action)
        {

        }



        public static TRet PerformInUow<TRet>(Func<TableContext, TRet> action)
        {
            var ret = default(TRet);



            return ret;
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
