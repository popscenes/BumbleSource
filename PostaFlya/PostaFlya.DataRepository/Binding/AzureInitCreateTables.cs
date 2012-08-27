using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Microsoft.WindowsAzure.StorageClient;
using Ninject;
using Ninject.Syntax;
using WebSite.Azure.Common.TableStorage;
using PostaFlya.DataRepository.Behaviour.TaskJob;
using PostaFlya.DataRepository.Browser;
using PostaFlya.DataRepository.Content;
using PostaFlya.DataRepository.Flier;
using PostaFlya.DataRepository.Search.Implementation;
using WebSite.Infrastructure.Util;

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

            iocContainer.Get<SqlSeachDbInitializer>().Initialize();

        }
    }
}
