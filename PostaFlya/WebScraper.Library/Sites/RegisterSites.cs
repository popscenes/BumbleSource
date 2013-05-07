using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Ninject.Modules;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using WebScraper.Library.Infrastructure;

namespace WebScraper.Library.Sites
{
    public class RegisterSites : NinjectModule
    {
        public const string SpottedMallard = "SpottedMallard";
        public const string Retreat = "Retreat";

        public override void Load()
        {
            Kernel.Bind<IWebDriver>().To<ChromeDriver>().InTransientScope();

            var types = Assembly.GetExecutingAssembly()
                                .DefinedTypes
                                .Where(t => t.GetInterfaces().Any(arg => arg == typeof (SiteScraperInterface)));
            foreach (var typeInfo in types)
            {
                Kernel.Bind(typeof (SiteScraperInterface)).To(typeInfo);
            }
        }
    }
}
