using System;
using System.Collections.Generic;
using MbUnit.Framework;
using Ninject;
using Ninject.MockingKernel.Moq;
using Website.Infrastructure.Command;
using Website.Domain.Browser;
using Website.Domain.Browser.Command;
using Website.Domain.Browser.Query;

namespace Website.Mocks.Domain.Data
{
    public static class BrowserTestData
    {
        public static void AssertStoreRetrieve(BrowserInterface storedBrowser, BrowserInterface retrievedBrowser)
        {
            Assert.AreEqual<string>(storedBrowser.Id, retrievedBrowser.Id);

            Assert.AreEqual<string>(storedBrowser.Handle, retrievedBrowser.Handle);
            Assert.AreEqual(storedBrowser.DefaultLocation, retrievedBrowser.DefaultLocation);
            Assert.AreEqual(storedBrowser.SavedLocations, retrievedBrowser.SavedLocations);
            Assert.AreElementsEqualIgnoringOrder(storedBrowser.SavedTags, retrievedBrowser.SavedTags);
            Assert.AreElementsEqualIgnoringOrder<string>(storedBrowser.Roles, retrievedBrowser.Roles);
            Assert.AreElementsEqualIgnoringOrder(storedBrowser.ExternalCredentials, retrievedBrowser.ExternalCredentials);
            Assert.AreEqual<string>(storedBrowser.FirstName, retrievedBrowser.FirstName);
            Assert.AreEqual<string>(storedBrowser.MiddleNames, retrievedBrowser.MiddleNames);
            Assert.AreEqual<string>(storedBrowser.Surname, retrievedBrowser.Surname);
            Assert.AreEqual(storedBrowser.Address, retrievedBrowser.Address);
            Assert.AreEqual<string>(storedBrowser.AvatarImageId, retrievedBrowser.AvatarImageId);
            Assert.AreEqual<bool>(storedBrowser.AddressPublic, retrievedBrowser.AddressPublic);
            Assert.AreEqual(storedBrowser.Address, retrievedBrowser.Address);
            Assert.AreEqual<string>(storedBrowser.EmailAddress, retrievedBrowser.EmailAddress);
        }

        public static BrowserInterface AssertGetById(BrowserInterface browser, BrowserQueryServiceInterface queryService)
        {
            var retrievedBrowser = queryService.FindById<Browser>(browser.Id);
            AssertStoreRetrieve(browser, retrievedBrowser);

            return retrievedBrowser;
        }

        public static BrowserInterface StoreOne(BrowserInterface browser, BrowserRepositoryInterface repository, StandardKernel kernel)
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

        public static void UpdateOne(BrowserInterface browser, BrowserRepositoryInterface repository, StandardKernel kernel)
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
