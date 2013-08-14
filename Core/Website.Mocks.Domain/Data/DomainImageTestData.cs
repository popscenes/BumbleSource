using System;
using System.Collections.Generic;
using NUnit.Framework;
using Ninject;
using Ninject.MockingKernel.Moq;
using Website.Domain.Content.Event;
using Website.Infrastructure.Command;
using Website.Domain.Browser;
using Website.Domain.Content;
using Website.Domain.Location;
using Website.Infrastructure.Domain;
using Website.Infrastructure.Messaging;
using Website.Infrastructure.Publish;
using Website.Infrastructure.Query;

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

        internal static ImageInterface AssertGetById(ImageInterface image, GenericQueryServiceInterface queryService)
        {
            var retrievedFlier = queryService.FindById<Image>(image.Id);
            AssertStoreRetrieve(image, retrievedFlier);

            return retrievedFlier;
        }


        internal static ImageInterface StoreOne(Image image, GenericRepositoryInterface repository, StandardKernel kernel)
        {
            var uow = kernel.Get<UnitOfWorkFactoryInterface>()
                .GetUnitOfWork(new List<RepositoryInterface>() {repository});
            using (uow)
            {

                repository.Store(image);
            }

            if (uow.Successful)
            {
                var indexers = kernel.GetAll<HandleEventInterface<ImageModifiedEvent>>();
                foreach (var handleEvent in indexers)
                {
                    handleEvent.Handle(new ImageModifiedEvent() { NewState = image });
                }
            }

            Assert.IsTrue(uow.Successful);
            return image;
        }

        internal static void UpdateOne(ImageInterface image, GenericRepositoryInterface repository, StandardKernel kernel)
        {
            ImageInterface oldState = null;
            UnitOfWorkInterface unitOfWork;
            using (unitOfWork = kernel.Get<UnitOfWorkFactoryInterface>().GetUnitOfWork(new List<RepositoryInterface>() { repository }))
            {
                repository.UpdateEntity<Image>(image.Id, e =>
                    {
                        oldState = e.CreateCopy<Image, Image>(ImageInterfaceExtensions.CopyFieldsFrom);
                        e.CopyFieldsFrom(image);
                    });
            }

            if (unitOfWork.Successful)
            {
                var indexers = kernel.GetAll<HandleEventInterface<ImageModifiedEvent>>();
                foreach (var handleEvent in indexers)
                {
                    handleEvent.Handle(new ImageModifiedEvent() { NewState = (Image)image, OrigState = (Image)oldState });
                }
            }
        }

        public static Image GetOne(MoqMockingKernel kernel, string browserid = null)
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
