using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MbUnit.Framework;
using Ninject;
using Ninject.MockingKernel.Moq;
using PostaFlya.Domain.Browser;
using PostaFlya.Domain.Browser.Command;
using PostaFlya.Domain.Content;
using PostaFlya.Domain.Content.Command;
using PostaFlya.Domain.Content.Query;
using PostaFlya.Domain.Location;
using WebSite.Infrastructure.Command;

namespace PostaFlya.Mocks.Domain.Data
{
    public static class DomainImageTestData
    {
        public static void AssertStoreRetrieve(ImageInterface storedImageInfo, ImageInterface retrievedImageInfo)
        {
            Assert.AreEqual(storedImageInfo.Id, retrievedImageInfo.Id);
            Assert.AreEqual(storedImageInfo.Location, retrievedImageInfo.Location);
            Assert.AreEqual(storedImageInfo.BrowserId, retrievedImageInfo.BrowserId);
            Assert.AreEqual(storedImageInfo.Title, retrievedImageInfo.Title);
            Assert.AreEqual(storedImageInfo.Status, retrievedImageInfo.Status);
        }

        internal static ImageInterface AssertGetById(ImageInterface image, ImageQueryServiceInterface queryService)
        {
            var retrievedFlier = queryService.FindById(image.Id);
            AssertStoreRetrieve(image, retrievedFlier);

            return retrievedFlier;
        }


        internal static ImageInterface StoreOne(ImageInterface image, ImageRepositoryInterface repository, StandardKernel kernel)
        {
            using (kernel.Get<UnitOfWorkFactoryInterface>()
                .GetUnitOfWork(new List<RepositoryInterface>() { repository }))
            {

                repository.Store(image);
            }
            return image;
        }

        internal static void UpdateOne(ImageInterface image, ImageRepositoryInterface repository, StandardKernel kernel)
        {
            using (kernel.Get<UnitOfWorkFactoryInterface>()
                .GetUnitOfWork(new List<RepositoryInterface>() { repository }))
            {
                repository.UpdateEntity(image.Id, e => e.CopyFieldsFrom(image));
            }
        }

        public static ImageInterface GetOne(MoqMockingKernel kernel, string browserid = null)
        {
            browserid = browserid ?? kernel.Get<BrowserInterface>(ctx => ctx.Has("defaultbrowser")).Id;
            return new Image()
                       {
                            Id = Guid.NewGuid().ToString(),
                            BrowserId = browserid,
                            Location = new Location(123,123),
                            Title = "This is an image",
                            Status = ImageStatus.Ready
                       };
        }
    }
}
