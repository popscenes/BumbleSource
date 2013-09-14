using System.Net;
using Microsoft.WindowsAzure.Storage.Table;
using Ninject.Syntax;
//using PostaFlya.DataRepository.Search.Implementation;
using Website.Infrastructure.Util;

namespace PostaFlya.DataRepository.Binding
{
    public class AzureInitCreateTables : InitServiceInterface
    {
        private readonly CloudTableClient _tableClient;

        public AzureInitCreateTables(CloudTableClient tableClient)
        {
            _tableClient = tableClient;
        }

        public void Init(IResolutionRoot iocContainer)
        {
            
//            iocContainer.Get<AzureTableContext>("browser").InitFirstTimeUse();
//            iocContainer.Get<AzureTableContext>("flier").InitFirstTimeUse();
//            iocContainer.Get<AzureTableContext>("image").InitFirstTimeUse();
//            iocContainer.Get<AzureTableContext>("taskjob").InitFirstTimeUse();
//            iocContainer.Get<AzureTableContext>("comments").InitFirstTimeUse();
//            iocContainer.Get<AzureTableContext>("claims").InitFirstTimeUse();


            //not sure if this matters but just put here because of below article
            //http://social.msdn.microsoft.com/Forums/en-US/windowsazuredata/thread/d84ba34b-b0e0-4961-a167-bbe7618beb83
            ServicePointManager.DefaultConnectionLimit = 48;
            ServicePointManager.Expect100Continue = false;
            ServicePointManager.UseNagleAlgorithm = false;

            //iocContainer.Get<SqlSeachDbInitializer>().Initialize();

        }
    }
}
