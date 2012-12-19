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
using Website.Application.Authentication;
using Website.Application.Azure.Caching;
using Website.Application.Binding;
using Website.Application.Caching.Command;
using Website.Application.Command;
using Website.Application.Content;
using PostaFlya.Areas.Default.Binding;
using PostaFlya.Areas.TaskJob.Binding;
using Website.Azure.Common.Environment;
using Website.Infrastructure.Authentication;
using Website.Infrastructure.Binding;
using Website.Infrastructure.Command;
using Website.Infrastructure.Util;
using PostaFlya.Models.Factory;
using Website.Application.Domain.Content;
using Website.Domain.Content;

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


            //if we need a broadcast bus injected in request scope
//            Func<string> broadcastEndpointId = () => CryptoUtil.CalculateHash(AzureEnv.GetIdForInstance());
//            Bind<Func<string>>().ToConstant(broadcastEndpointId).WithMetadata("BroadcastCommunicator", true);
           
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
                      new Website.Infrastructure.Binding.InfrastructureNinjectBinding(),
                      new PostaFlya.Domain.Binding.DefaultServicesNinjectBinding(),      
                      new PostaFlya.Domain.Binding.CommandNinjectBinding(),

                      new Website.Domain.Binding.DefaultServicesNinjectBinding(),      
                      new Website.Domain.Binding.CommandNinjectBinding(),

                      new PostaFlya.Domain.TaskJob.Binding.TaskJobNinjectBinding(),
                      new Website.Azure.Common.Binding.AzureCommonNinjectBinding(),
                      //this binds the caching repositories
                      new PostaFlya.Application.Domain.Binding.ApplicationDomainRepositoriesNinjectBinding(
                          c => c.InRequestScope()),

                          new Website.Application.Domain.Binding.ApplicationDomainRepositoriesNinjectBinding(
                          c => c.InRequestScope()),
                      //this just binds for source repositories
                      new PostaFlya.DataRepository.Binding.AzureRepositoryNinjectBinding(
                          c => { c.InRequestScope();
                                 c.WhenTargetHas<SourceDataSourceAttribute>();
                      }),                      
                      new Website.Application.Binding.ApplicationCommandHandlersNinjectBinding(),
                      new Website.Application.Binding.ApplicationNinjectBinding(),
                      new PostaFlya.Application.Domain.Binding.ApplicationDomainNinjectBinding(),
                      new PostaFlya.Application.Domain.Binding.ApplicationDomainServicesNinjectBinding(),
                      new Website.Application.Domain.Binding.ApplicationDomainNinjectBinding(),
                      new Website.Application.Azure.Binding.AzureApplicationNinjectBinding(),
                      new PostaFlya.Binding.WebNinjectBindings(),
                      new PostaFlya.Areas.Default.Binding.DefaultBehaviourWebNinjectBinding(),
                      new PostaFlya.Areas.TaskJob.Binding.TaskJobBehaviourWebNinjectBinding(),
                      new PostaFlya.DataRepository.Binding.TableNameNinjectBinding(),
                  };
    }
}