using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Ninject;
using Ninject.Modules;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using WebScraper.Library.Infrastructure;
using Website.Infrastructure.Configuration;

namespace WebScraper.Library.Sites
{
    public class RegisterSites : NinjectModule
    {
        public const string SpottedMallard = "SpottedMallard";
        public const string Retreat = "Retreat";
        public const string DrunkenPoet = "DrunkenPoet";
        public const string GraceDarling = "GraceDarling";
        public const string DingDong = "DingDong";

        public override void Load()
        {
            Bind<ConfigurationServiceInterface>()
                .To<DefaultConfigurationService>()
                .InSingletonScope();
            Config.Instance = Kernel.Get<ConfigurationServiceInterface>();

            Kernel.Bind<IWebDriver>().To<ChromeDriver>().InTransientScope();

            //            var types = Assembly.GetExecutingAssembly()
            //                                .DefinedTypes
            //                                .Where(t => t.GetInterfaces().Any(arg => arg == typeof (SiteScraperInterface)));
            //
            //            foreach (var typeInfo in types)
            //            {
            //                Kernel.Bind(typeof (SiteScraperInterface)).To(typeInfo);
            //            }

            Kernel.Bind<SiteScraperInterface>()
                  .To<SpottedMallardSiteScraper>()
                  .Named(SpottedMallardSiteScraper.BaseUrl);

            Kernel.Bind<SiteScraperInterface>()
                  .To<RetreatSiteScraper>()
                  .Named(RetreatSiteScraper.BaseUrl);

            Kernel.Bind<SiteScraperInterface>()
            .To<DrunkenPoetSiteScraper>()
            .Named(DrunkenPoetSiteScraper.BaseUrl);

            Kernel.Bind<SiteScraperInterface>()
            .To<GraceDarlingScraper>()
            .Named(GraceDarlingScraper.BaseUrl);

            Kernel.Bind<SiteScraperInterface>()
            .To<DingDongScraper>()
            .Named(DingDongScraper.BaseUrl);
        }
    }
}
