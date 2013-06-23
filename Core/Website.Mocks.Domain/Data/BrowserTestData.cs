using System;
using System.Collections.Generic;
using NUnit.Framework;
using Ninject;
using Ninject.MockingKernel.Moq;
using Website.Domain.Contact;
using Website.Infrastructure.Command;
using Website.Domain.Browser;
using Website.Infrastructure.Query;

namespace Website.Mocks.Domain.Data
{
    public static class ContactDetailTestData
    {
        public static void AssertStoreRetrieve(ContactDetailsInterface storedContactDetails, ContactDetailsInterface retrievedContactDetails)
        {
            Assert.AreEqual(storedContactDetails.PhoneNumber, retrievedContactDetails.PhoneNumber);
            Assert.AreEqual(storedContactDetails.FirstName, retrievedContactDetails.FirstName);
            Assert.AreEqual(storedContactDetails.MiddleNames, retrievedContactDetails.MiddleNames);
            Assert.AreEqual(storedContactDetails.Surname, retrievedContactDetails.Surname);
            Assert.AreEqual(storedContactDetails.Address, retrievedContactDetails.Address);
            Assert.AreEqual(storedContactDetails.EmailAddress, retrievedContactDetails.EmailAddress);
            Assert.AreEqual(storedContactDetails.WebSite, retrievedContactDetails.WebSite);
               
        }
    }
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

        public static BrowserInterface StoreOne(BrowserInterface browser, GenericRepositoryInterface repository, StandardKernel kernel, bool checkuow = true)
        {
            var uow = kernel.Get<UnitOfWorkFactoryInterface>()
                .GetUnitOfWork(new List<RepositoryInterface>() {repository});
            using (uow)
            {

                repository.Store(browser);
            }

            if (checkuow)
                Assert.IsTrue(uow.Successful);
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
            var ret = kernel.Get<BrowserInterface>(ctx => ctx.Has("defaultbrowser"));
            ret.Id = Guid.NewGuid().ToString();
            return ret;
        }
    }
}
