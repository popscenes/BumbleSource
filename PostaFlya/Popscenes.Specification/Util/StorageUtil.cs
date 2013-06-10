using System;
using System.Collections.Generic;
using System.Data.Services.Client;
using Ninject;
using PostaFlya.DataRepository.Search.Implementation;
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

            tableNameProv.SuffixTableNames("test");

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
    }
}
