using System;
using System.Collections.Generic;
using NUnit.Framework;
using Ninject;
using Ninject.MockingKernel.Moq;
using Website.Infrastructure.Command;
using Website.Domain.Browser;
using Website.Domain.Content;
using Website.Domain.Content.Command;
using Website.Domain.Content.Query;
using Website.Domain.Location;

namespace Website.Mocks.Domain.Data
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
            var retrievedFlier = queryService.FindById<Image>(image.Id);
            AssertStoreRetrieve(image, retrievedFlier);

            return retrievedFlier;
        }


        internal static ImageInterface StoreOne(ImageInterface image, ImageRepositoryInterface repository, StandardKernel kernel)
        {
            var uow = kernel.Get<UnitOfWorkFactoryInterface>()
                .GetUnitOfWork(new List<RepositoryInterface>() {repository});
            using (uow)
            {

                repository.Store(image);
            }

            Assert.IsTrue(uow.Successful);
            return image;
        }

        internal static void UpdateOne(ImageInterface image, ImageRepositoryInterface repository, StandardKernel kernel)
        {
            using (kernel.Get<UnitOfWorkFactoryInterface>()
                .GetUnitOfWork(new List<RepositoryInterface>() { repository }))
            {
                repository.UpdateEntity<Image>(image.Id, e => e.CopyFieldsFrom(image));
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
