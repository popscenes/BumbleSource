﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Caching;
using System.Security.Principal;
using System.Web.Http;
using System.Web.Mvc;
using Ninject;
using Ninject.Modules;
using Ninject.Web.Common;
using Website.Application.Authentication;
using Website.Application.Azure.Caching;
using Website.Application.Domain.Google.Payment;
using Website.Application.Domain.Payment;
using Website.Application.Google.Payment;
using Website.Azure.Common.Environment;
using Website.Common.Binding;
using Website.Infrastructure.Authentication;
using Website.Infrastructure.Binding;
using Website.Infrastructure.Command;
using Website.Infrastructure.Configuration;
using Website.Application.Domain.Content;
using Website.Domain.Content;
using Website.Infrastructure.Messaging;

namespace PostaFlya.Binding
{
    public class WebNinjectBindings : NinjectModule
    {
        #region Overrides of NinjectModule

        public override void Load()
        {
            Trace.TraceInformation("Binding WebNinjectBindings");

            //command bus
            Bind<MessageBusInterface>()
                .To<InMemoryMessageBus>().WhenInjectedInto<Controller>()
                .InSingletonScope();
            Bind<MessageBusInterface>()
                .To<InMemoryMessageBus>().WhenInjectedInto<ApiController>()
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

            Bind<PaymentServiceProviderInterface>()
                .To<PaymentServiceProvider>()
                .InSingletonScope();

            var paymentServiveProvider = Kernel.Get<PaymentServiceProviderInterface>();
            var config = Kernel.Get<ConfigurationServiceInterface>();
            /*var paypal = new PaypalExpressCheckout();
            if (Config.Instance != null)
            {
                paypal = new PaypalExpressCheckout()
                    {
                        ApiEndpoint = config.GetSetting("PaypalAPIEndpoint"),
                        Url = config.GetSetting("PaypalUrl"),
                        Name = config.GetSetting("PaypalName"),
                        Password = config.GetSetting("PaypalPassword"),
                        Signiture = config.GetSetting("PaypalSigniture"),
                        Version = config.GetSetting("PaypalVersion"),
                        CallbackUrl = config.GetSetting("SiteUrl") + config.GetSetting("PaypalCallbackUrl"),
                        CancelUrl = config.GetSetting("SiteUrl") + config.GetSetting("PaypalCancelUrl")
                    };
            }
            paymentServiveProvider.Add(new PaypalPaymentService(){ PaypalExpressCheckout = paypal});*/

            if (Config.Instance != null)
            {
                var googleWallet = new GoogleWalletDigitalGoods()
                    {
                        Secret = config.GetSetting("GoogleWalletSecret"),
                        SellerId = config.GetSetting("GoogleWalletSellerId"),
                        PaymentUrl = config.GetSetting("GooglePaymentUrl"),
                    };
                paymentServiveProvider.Add(new GoogleWalletPaymentService() {GoogleWalletDigitalGoods = googleWallet});
            }



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
            Func<ObjectCache> getInMemCache = () =>
                {
                    var cacheSettings = new NameValueCollection(3)
                        {
                            {"CacheMemoryLimitMegabytes", Convert.ToString(0)},
                            {"physicalMemoryLimitPercentage", Convert.ToString(49)},
                            {"pollingInterval", Convert.ToString("00:00:30")}
                        };
                    return new MemoryCache("WebSiteCache", cacheSettings);
                };

            Bind<ObjectCache>()
                .ToMethod(ctx =>
                {
                    var ret = AzureEnv.IsRunningInCloud() ? new AzureCacheProvider() : getInMemCache();
                    //var ret = new AzureCacheProvider();
                    return ret;
                }).InSingletonScope();
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



            BindViewModelMappers(Kernel);

            Trace.TraceInformation("Finished Binding WebNinjectBindings");

        }

        public static void BindViewModelMappers(IKernel kernel)
        {
            kernel.BindViewModelMappersFromCallingAssembly();
            kernel.BindMessageAndQueryHandlersFromCallingAssembly(c => c.InTransientScope());

            kernel.BindWebsiteCommonQueryHandlersForTypesFrom(c => c.InRequestScope()
                 , Assembly.GetAssembly(typeof(InfrastructureNinjectBinding))
                 , Assembly.GetAssembly(typeof(WebNinjectBindings))
                 , Assembly.GetAssembly(typeof(PostaFlya.Domain.Flier.Flier))
                 , Assembly.GetAssembly(typeof(Website.Domain.Claims.Claim)));

//            kernel.BindInfrastructureQueryHandlersForTypesFrom(
//                     c => c.InRequestScope()
//                     , Assembly.GetAssembly(typeof(WebNinjectBindings)));

//
//            kernel.BindGenericQueryHandlersFromAssemblyForTypesFrom(Assembly.GetAssembly(typeof(IsModelInterface))
//         , c => c.InRequestScope()
//         , t => typeof(IsModelInterface).IsAssignableFrom(t)
//             || typeof(EntityIdInterface).IsAssignableFrom(t)
//             || typeof(QueryInterface).IsAssignableFrom(t)
//         , null
//         , Assembly.GetAssembly(typeof(WebNinjectBindings))
//         , Assembly.GetAssembly(typeof(PostaFlya.Domain.Flier.Flier))
//         , Assembly.GetAssembly(typeof(Website.Domain.Claims.Claim))
//         , Assembly.GetAssembly(typeof(FindByIdQuery<>)));
        }

        #endregion
    }

    public static class PopscenesNinjectBindings
    {
        public static readonly List<INinjectModule> NinjectModules = new List<INinjectModule>()
                  {
                      new Website.Infrastructure.Binding.InfrastructureNinjectBinding(),
                      new PostaFlya.Domain.Binding.DefaultServicesNinjectBinding(),      
                      new PostaFlya.Domain.Binding.CommandNinjectBinding(),

                      new Website.Domain.Binding.DefaultServicesNinjectBinding(),      
                      new Website.Domain.Binding.CommandNinjectBinding(),

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
                      new PostaFlya.DataRepository.Binding.TableNameNinjectBinding(),
                      new Website.Application.Domain.Binding.ApplicationJobs(),
                      new PostaFlya.Application.Domain.Binding.ApplicationJobs(),
                      new Website.Common.Binding.WebsiteCommonNinjectBinding()
                  };
    }
}