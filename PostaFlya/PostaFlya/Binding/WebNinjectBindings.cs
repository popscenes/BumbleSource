using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Caching;
using System.Security.Principal;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using Ninject;
using Ninject.Modules;
using Ninject.Web.Common;
using WebSite.Application.Authentication;
using WebSite.Application.Azure.Caching;
using WebSite.Application.Binding;
using WebSite.Application.Caching.Command;
using WebSite.Application.Command;
using WebSite.Application.Communication;
using WebSite.Application.Content;
using PostaFlya.Application.Domain.Browser;
using PostaFlya.Application.Domain.Content;
using PostaFlya.Areas.Default.Binding;
using PostaFlya.Areas.TaskJob.Binding;
using WebSite.Azure.Common.Environment;
using PostaFlya.Domain.Content;
using WebSite.Infrastructure.Authentication;
using WebSite.Infrastructure.Binding;
using WebSite.Infrastructure.Command;
using WebSite.Infrastructure.Util;
using PostaFlya.Models.Factory;

namespace PostaFlya.Binding
{
    public class WebNinjectBindings : NinjectModule
    {
        #region Overrides of NinjectModule

        public override void Load()
        {
            Trace.TraceInformation("Binding WebNinjectBindings");

            //command bus
            Bind<CommandBusInterface>()
                .To<DefaultCommandBus>().WhenInjectedInto<Controller>()
                .InSingletonScope();
            Bind<CommandBusInterface>()
                .To<DefaultCommandBus>().WhenInjectedInto<ApiController>()
                .InSingletonScope();

            Bind<IIdentity>()
                .To<WebIdentity>();
            
            Bind<IdentityProviderServiceInterface>()
               .To<WebIdentityProviderService>();

            //image retriever
            Bind<RequestContentRetrieverInterface>().To<WebImageRequestContentRetriever>()
                .WithMetadata(Content.ContentType.Image.ToString(), true);

            Bind<UrlContentRetrieverInterface>().To<ImageUrlContentRetriever>()
                .WithMetadata(Content.ContentType.Image.ToString(), true);


//in memory caching
//            Bind<ObjectCache>()
//                .ToMethod(ctx =>
//                              {
//                                  var cacheSettings = new NameValueCollection(3)
//                                                          {
//                                                              {"CacheMemoryLimitMegabytes", Convert.ToString(0)},
//                                                              {"physicalMemoryLimitPercentage", Convert.ToString(49)},
//                                                              {"pollingInterval", Convert.ToString("00:00:30")}
//                                                          };
//                                  return new MemoryCache("WebSiteCache", cacheSettings);
//                              }).InSingletonScope();

              //bind cache notifications to the broadcast bus  
//              Bind<CommandBusInterface>().ToMethod(context =>
//                  context.Kernel.Get<BroadcastCommunicatorInterface>(metadata => metadata.Has("BroadcastCommunicator")))
//                  .WhenTargetHas<CachedNotificationBusAttribute>()
//                  .InRequestScope();

// Azure caching
            Bind<ObjectCache>()
                .ToMethod(ctx =>
                {
                    var ret =  new AzureCacheProvider();
                    return ret;
                }).InSingletonScope();
            //turn off notifications for cached repositories when using azure cache          
            Bind<CacheNotifier>().ToMethod(context => new CacheNotifier(null, false));
//end azure caching
            
            //broadcast communicator id func
            Func<string> broadcastEndpointId = () => CryptoUtil.CalculateHash(AzureEnv.GetIdForInstance());
            Bind<Func<string>>().ToConstant(broadcastEndpointId).WithMetadata("BroadcastCommunicator", true);

            //if we need a broadcast bus injected in request scope
//            Bind<CommandBusInterface>()
//                .ToMethod(ctx => ctx.Kernel.Get<CommandBusInterface>(metadata => metadata.Has("BroadcastCommunicator")))
//            .WhenTargetHas<BroadcastCommunicatorAttribute>()
//            .InRequestScope();

//            Bind<CommandBusInterface>()
//                .ToMethod(ctx =>
//                {
//                    var endpoint = CryptoUtil.CalculateHash(AzureEnv.GetIdForInstance());
//                    var fact = ctx.Kernel.Get<BroadcastCommunicatorFactoryInterface>();
//                    return fact.GetCommunicatorForEndpoint(endpoint);
//                })
//                .WithMetadata("BroadcastCommunicator", true);
//
//            Bind<QueuedCommandScheduler>()
//                .ToMethod(ctx =>
//                            {
//                                var endpoint = CryptoUtil.CalculateHash(AzureEnv.GetIdForInstance());
//                                var fact = ctx.Kernel.Get<BroadcastCommunicatorFactoryInterface>();
//                                return fact.GetCommunicatorForEndpoint(endpoint)
//                                    .GetScheduler();
//                            })
//                            .WithMetadata("BroadcastCommunicator", true);

            Bind<FlierBehaviourViewModelFactory>()
                .ToSelf().InSingletonScope();
            Bind<FlierBehaviourViewModelFactoryInterface>()
                .ToConstant(Kernel.Get<FlierBehaviourViewModelFactory>())
                .InSingletonScope();
            //retrieve and add view model factories for the different behaviours
            Bind<FlierBehaviourViewModelFactoryRegistryInterface>()
                .ToConstant(Kernel.Get<FlierBehaviourViewModelFactory>())
                .InSingletonScope();

            Trace.TraceInformation("Finished Binding WebNinjectBindings");

        }

        #endregion
    }

    public static class AllBindings
    {
        public static readonly List<INinjectModule> NinjectModules = new List<INinjectModule>()
                  {
                      new WebSite.Infrastructure.Binding.InfrastructureNinjectBinding(),
                      new PostaFlya.Domain.Binding.DefaultServicesNinjectBinding(),      
                      new PostaFlya.Domain.Binding.CommandNinjectBinding(),
                      new PostaFlya.Domain.TaskJob.Binding.TaskJobNinjectBinding(),
                      new WebSite.Azure.Common.Binding.AzureCommonNinjectBinding(),
                      //this binds the caching repositories
                      new PostaFlya.Application.Domain.Binding.ApplicationDomainRepositoriesNinjectBinding(
                          c => c.InRequestScope()),
                      //this just binds for source repositories
                      new PostaFlya.DataRepository.Binding.AzureRepositoryNinjectBinding(
                          c => { c.InRequestScope();
                                 c.WhenTargetHas<SourceDataSourceAttribute>();
                      }),
                      new PostaFlya.DataRepository.Binding.TableNameNinjectBinding(),
                      new WebSite.Application.Binding.ApplicationNinjectBinding(),
                      new PostaFlya.Application.Domain.Binding.ApplicationDomainNinjectBinding(),
                      new WebSite.Application.Azure.Binding.AzureApplicationNinjectBinding(),
                      new PostaFlya.Binding.WebNinjectBindings(),
                      new PostaFlya.Areas.Default.Binding.DefaultBehaviourWebNinjectBinding(),
                      new PostaFlya.Areas.TaskJob.Binding.TaskJobBehaviourWebNinjectBinding()
                  };
    }
}