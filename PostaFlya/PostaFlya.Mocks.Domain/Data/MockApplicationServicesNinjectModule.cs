using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Moq;
using NUnit.Framework;
using Ninject;
using Ninject.MockingKernel.Moq;
using Ninject.Modules;
using PostaFlya.Application.Domain.ExternalSource;
using PostaFlya.Application.Domain.Flier;
using PostaFlya.Domain.Flier;
using Website.Application.Domain.Content;
using Website.Domain.TinyUrl;
using Website.Infrastructure.Authentication;
using Website.Domain.Browser;
using Website.Infrastructure.Domain;
using Website.Infrastructure.Query;

namespace PostaFlya.Mocks.Domain.Data
{
    public class MockApplicationServicesNinjectModule: NinjectModule
    {
        public override void Load()
        {
            var kernel = Kernel as MoqMockingKernel;
            Assert.IsNotNull(kernel, "should be using mock kernel for tests");

            SetUpTestExternalSourceServices(kernel);
            SetUpFlierPrintService(kernel);

            Kernel.Bind<FlierWebAnalyticServiceInterface>()
                  .To<DefaultFlierWebAnalyticService>()
                  .InTransientScope();
        }

        public static void SetUpTinyUrlService<EnityType>(MoqMockingKernel kernel)
            where EnityType : EntityInterface, TinyUrlInterface
        {
            var tinyUrlService = kernel.GetMock<TinyUrlServiceInterface>();
            tinyUrlService.Setup(service => service.UrlFor(It.IsAny<EnityType>()))
                .Returns("http://atiny.url/1");
            kernel.Bind<TinyUrlServiceInterface>()
                .ToConstant(tinyUrlService.Object);
        }

        private static void SetUpFlierPrintService(MoqMockingKernel kernel)
        {
            var printService = kernel.GetMock<FlierPrintImageServiceInterface>();
            
            printService.Setup(ps => ps.GetPrintImageForFlierWithTearOffs(It.IsAny<string>()))
                        .Returns<string>(s =>
                            {
                                var qs = kernel.Get<GenericQueryServiceInterface>();
                                return qs.FindById<Flier>(s) == null ? null 
                                    : new Bitmap(ImageUtil.A4300DpiSize.Width, ImageUtil.A4300DpiSize.Height);
                            });

            kernel.Bind<FlierPrintImageServiceInterface>()
                .ToConstant(printService.Object);
        }

        public void SetUpTestExternalSourceServices(MoqMockingKernel kernel)
        {
            var FlierImportService = kernel.GetMock<FlierImportServiceInterface>();
            var FlierImporter = kernel.GetMock<FlierImporterInterface>();

            FlierImporter.Setup(_ => _.ImportFliers(It.IsAny<BrowserInterface>())).Returns(() => GetImportedFliersMock());

            FlierImporter.Setup(_ => _.CanImport(It.Is<BrowserInterface>(p => p.ExternalCredentials.Where(e => e.IdentityProvider == IdentityProviders.FACEBOOK).First().AccessToken.Expires < DateTime.Now))).Returns(false);
            FlierImporter.Setup(_ => _.CanImport(It.Is<BrowserInterface>(p => p.ExternalCredentials.Where(e => e.IdentityProvider == IdentityProviders.FACEBOOK).First().AccessToken.Expires > DateTime.Now))).Returns(true);

            FlierImportService.Setup(_ => _.GetImporter(It.IsAny<String>())).Returns(FlierImporter.Object);

            kernel.Bind<FlierImportServiceInterface>().ToConstant(FlierImportService.Object).InSingletonScope();
        }

        protected IQueryable<Flier> GetImportedFliersMock()
        {
            List<Flier> flierList = new List<Flier>();
            
            for (int i = 0; i < 5; i++)
            {
                flierList.Add(new Flier() 
                    {
                        Status = FlierStatus.Pending,
                        Title = "Imported Flier " + i.ToString()
                    }
                );
            }

            return flierList.AsQueryable();
        }

    }
}
