using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using Ninject.Modules;
using WebSite.Azure.Common.TableStorage;
using PostaFlya.DataRepository.Behaviour.TaskJob;
using PostaFlya.DataRepository.Browser;
using PostaFlya.DataRepository.Content;
using PostaFlya.DataRepository.Flier;
using PostaFlya.DataRepository.Internal;

namespace PostaFlya.DataRepository.Binding
{
    public class TableNameNinjectBinding : NinjectModule
    {
        #region Overrides of NinjectModule

        public override void Load()
        {
            Trace.TraceInformation("Binding TableNameNinjectBinding");

            //table name bindings the new way to be used with AzureTableContext
            Kernel.Bind<TableNameAndPartitionProviderInterface>()
                .ToConstant(new TableNameAndPartitionProviderCollection()
                                {
                                    BrowserStorageDomain.TableNamesAndPartition,
                                    BrowserIdentityProviderCredential.TableNamesAndPartition
                                })
                .WhenAnyAnchestorNamed("browser");


            Kernel.Bind<TableNameAndPartitionProviderInterface>()
                .ToConstant(FlierStorageDomain.TableNamesAndPartition)
                .WhenAnyAnchestorNamed("flier");

            Kernel.Bind<TableNameAndPartitionProviderInterface>()
                .ToConstant(ImageStorageDomain.TableNamesAndPartition)
                .WhenAnyAnchestorNamed("image");

            Kernel.Bind<TableNameAndPartitionProviderInterface>()
                .ToConstant(TaskJobStorageDomain.TableNamesAndPartition)
                .WhenAnyAnchestorNamed("taskjob");

            Kernel.Bind<TableNameAndPartitionProviderInterface>()
                .ToConstant(CommentStorageDomain.TableNamesAndPartition)
                .WhenAnyAnchestorNamed("comments");

            Kernel.Bind<TableNameAndPartitionProviderInterface>()
               .ToConstant(LikeStorageDomain.TableNamesAndPartition)
               .WhenAnyAnchestorNamed("likes");

            Trace.TraceInformation("Finished Binding TableNameNinjectBinding");

        }

        #endregion
    }
}
