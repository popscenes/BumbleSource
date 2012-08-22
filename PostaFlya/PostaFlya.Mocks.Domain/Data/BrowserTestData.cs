using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MbUnit.Framework;
using Ninject;
using Ninject.MockingKernel.Moq;
using PostaFlya.Domain.Browser;
using PostaFlya.Domain.Browser.Command;
using PostaFlya.Domain.Browser.Query;
using WebSite.Infrastructure.Command;

namespace PostaFlya.Mocks.Domain.Data
{
    public static class BrowserTestData
    {
        public static void AssertStoreRetrieve(BrowserInterface storedBrowser, BrowserInterface retrievedBrowser)
        {
            Assert.AreEqual(storedBrowser.Id, retrievedBrowser.Id);

            Assert.AreEqual(storedBrowser.Handle, retrievedBrowser.Handle);
            Assert.AreEqual(storedBrowser.DefaultLocation, retrievedBrowser.DefaultLocation);
            Assert.AreEqual(storedBrowser.SavedLocations, retrievedBrowser.SavedLocations);
            Assert.AreElementsEqualIgnoringOrder(storedBrowser.SavedTags, retrievedBrowser.SavedTags);
            Assert.AreElementsEqualIgnoringOrder(storedBrowser.Roles, retrievedBrowser.Roles);
            Assert.AreElementsEqualIgnoringOrder(storedBrowser.ExternalCredentials, retrievedBrowser.ExternalCredentials);
            Assert.AreEqual(storedBrowser.FirstName, retrievedBrowser.FirstName);
            Assert.AreEqual(storedBrowser.MiddleNames, retrievedBrowser.MiddleNames);
            Assert.AreEqual(storedBrowser.Surname, retrievedBrowser.Surname);
            Assert.AreEqual(storedBrowser.Address, retrievedBrowser.Address);
            Assert.AreEqual(storedBrowser.AvatarImageId, retrievedBrowser.AvatarImageId);
            Assert.AreEqual(storedBrowser.AddressPublic, retrievedBrowser.AddressPublic);
            Assert.AreEqual(storedBrowser.Address, retrievedBrowser.Address);
            Assert.AreEqual(storedBrowser.EmailAddress, retrievedBrowser.EmailAddress);
        }

        internal static BrowserInterface AssertGetById(BrowserInterface browser, BrowserQueryServiceInterface queryService)
        {
            var retrievedFlier = queryService.FindById<Browser>(browser.Id);
            AssertStoreRetrieve(browser, retrievedFlier);

            return retrievedFlier;
        }

        internal static BrowserInterface StoreOne(BrowserInterface browser, BrowserRepositoryInterface repository, StandardKernel kernel)
        {
            var uow = kernel.Get<UnitOfWorkFactoryInterface>()
                .GetUnitOfWork(new List<RepositoryInterface>() {repository});
            using (uow)
            {

                repository.Store(browser);
            }

            Assert.IsTrue(uow.Successful);
            return browser;
        }

        internal static void UpdateOne(BrowserInterface browser, BrowserRepositoryInterface repository, StandardKernel kernel)
        {
            using (kernel.Get<UnitOfWorkFactoryInterface>()
                .GetUnitOfWork(new List<RepositoryInterface>() { repository }))
            {
                repository.UpdateEntity<Browser>(browser.Id, e => e.CopyFieldsFrom(browser));
            }
        }

        public static BrowserInterface GetOne(MoqMockingKernel kernel)
        {
            var ret = kernel.Get<BrowserInterface>(ctx => ctx.Has("defaultbrowser"));
            ret.Id = Guid.NewGuid().ToString();
            return ret;
        }
    }
}
