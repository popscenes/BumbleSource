using System;
using System.Collections.Generic;
using NUnit.Framework;
using Ninject;
using Ninject.MockingKernel.Moq;
using PostaFlya.Domain.Browser;
using Website.Infrastructure.Command;
using Website.Infrastructure.Query;
using Website.Mocks.Domain.Data;

namespace PostaFlya.Mocks.Domain.Data
{
    public static class BrowserTestData
    {
        public static void AssertStoreRetrieve(BrowserInterface storedBrowser, BrowserInterface retrievedBrowser)
        {
            Assert.AreEqual(storedBrowser.Id, retrievedBrowser.Id);

            ContactDetailTestData.AssertStoreRetrieve(storedBrowser, retrievedBrowser);
            Assert.AreEqual(storedBrowser.FriendlyId, retrievedBrowser.FriendlyId);
            CollectionAssert.AreEquivalent(storedBrowser.Roles, retrievedBrowser.Roles);
            CollectionAssert.AreEquivalent(storedBrowser.ExternalCredentials, retrievedBrowser.ExternalCredentials);
            Assert.AreEqual(storedBrowser.AvatarImageId, retrievedBrowser.AvatarImageId);
            Assert.AreEqual(storedBrowser.Address, retrievedBrowser.Address);
        }

        public static BrowserInterface AssertGetById(BrowserInterface browser, GenericQueryServiceInterface queryService)
        {
            var retrievedBrowser = queryService.FindById<Browser>(browser.Id);
            AssertStoreRetrieve(browser, retrievedBrowser);

            return retrievedBrowser;
        }

        public static BrowserInterface StoreOne(BrowserInterface browser, GenericRepositoryInterface repository, StandardKernel kernel)
        {
            var uow = kernel.Get<UnitOfWorkFactoryInterface>()
                            .GetUnitOfWork(new List<RepositoryInterface>() { repository });
            using (uow)
            {

                repository.Store(browser);
            }

            Assert.IsTrue(uow.Successful);

            Website.Mocks.Domain.Data.BrowserTestData.StoreOne(browser, repository, kernel, false);

            return browser;
        }

        public static void UpdateOne(BrowserInterface browser, GenericRepositoryInterface repository, StandardKernel kernel)
        {
            using (kernel.Get<UnitOfWorkFactoryInterface>()
                         .GetUnitOfWork(new List<RepositoryInterface>() { repository }))
            {
                repository.UpdateEntity<Browser>(browser.Id, e => e.CopyFieldsFrom(browser));
            }
        }

        public static BrowserInterface GetOne(MoqMockingKernel kernel)
        {
            var ret = kernel.Get<BrowserInterface>(ctx => ctx.Has("postadefaultbrowser"));
            ret.Id = Guid.NewGuid().ToString();
            return ret;
        }
    }
}