using System;
using System.Collections.Generic;
using System.Linq;
using MbUnit.Framework;
using Moq;
using Ninject.MockingKernel.Moq;
using Ninject.Modules;
using PostaFlya.Application.Domain.ExternalSource;
using PostaFlya.Domain.Flier;
using WebSite.Infrastructure.Authentication;
using Website.Domain.Browser;

namespace PostaFlya.Mocks.Domain.Data
{
    public class ExternalSourceNinjectModule: NinjectModule
    {
        public override void Load()
        {
            var kernel = Kernel as MoqMockingKernel;
            Assert.IsNotNull(kernel, "should be using mock kernel for tests");

            SetUpTestExternalSourceServices(kernel);
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
