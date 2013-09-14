using System.Collections.Generic;
using System.Data.Services.Client;
using Ninject;
//using PostaFlya.DataRepository.Search.Implementation;
using TechTalk.SpecFlow;
using Website.Application.Messaging;
using Website.Azure.Common.Environment;
using Website.Azure.Common.TableStorage;

namespace Popscenes.Specification.Util
{
    public static class StorageUtil
    {
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

            //SpecUtil.Kernel.Get<SqlSeachDbInitializer>().DeleteAll();

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

            //SpecUtil.Kernel.Get<SqlSeachDbInitializer>().Initialize();

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
