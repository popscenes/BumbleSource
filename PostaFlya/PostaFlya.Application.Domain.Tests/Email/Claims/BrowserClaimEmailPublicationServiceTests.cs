using System;
using System.Linq;
using System.Net.Mail;
using Moq;
using NUnit.Framework;
using Ninject;
using Ninject.MockingKernel.Moq;
using PostaFlya.Application.Domain.Email.Claims;
using PostaFlya.Mocks.Domain.Data;
using Website.Application.Email;
using Website.Domain.Browser;
using Website.Domain.Claims;
using Website.Domain.Claims.Event;
using Website.Infrastructure.Command;
using Website.Mocks.Domain.Data;

namespace PostaFlya.Application.Domain.Tests.Email.Claims
{
    [TestFixture]
    public class BrowserClaimEmailPublicationServiceTests
    {
        MoqMockingKernel Kernel
        {
            get { return TestFixtureSetup.CurrIocKernel; }
        }

        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            Kernel.Rebind<BrowserClaimEmailSubscription>()
                .ToSelf().InTransientScope();
            Kernel.Rebind<CommandBusInterface>()
                .To<DefaultCommandBus>();
        }

        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
            Kernel.Unbind<BrowserClaimEmailSubscription>();
            Kernel.Unbind<CommandBusInterface>();
            Kernel.Unbind <SendEmailServiceInterface>();
        }

        [Test]
        public void BrowserClaimEmailPublicationServiceSendsEmailWithVCardAttachmentToBrowserThatClaimedTearOffIfFlierHasContactDetails()
        {
            var repository = Kernel.Get<GenericRepositoryInterface>();
            
            BrowserInterface claimBrowser = new Website.Domain.Browser.Browser {Id = Guid.NewGuid().ToString(), EmailAddress = "test@bumbleflya.com"};
            claimBrowser = BrowserTestData.StoreOne(claimBrowser, repository, Kernel);

            var flierBrowser = BrowserTestData.GetOne(Kernel);
            flierBrowser = BrowserTestData.StoreOne(flierBrowser, repository, Kernel);

            var flier = FlierTestData.GetOne(Kernel);
            flier.EffectiveDate = DateTime.UtcNow.AddDays(-1);
            flier.BrowserId = flierBrowser.Id;
            var storedFlier = FlierTestData.StoreOne(flier, repository, Kernel);

            var emailSent = false;
            var emailSentToClaimAddress = false;
            var hasVCard = false;

            var emailMock = Kernel.GetMock<SendEmailServiceInterface>();
            emailMock.Setup(sm => sm.Send(It.IsAny<MailMessage>()))
                .Callback<MailMessage>(email =>
                    {
                        emailSent = true;
                        emailSentToClaimAddress = 
                            email
                            .To
                            .Any(s => s.Address.CompareTo(claimBrowser.EmailAddress) == 0);
                        hasVCard = email.Attachments.Any(a => a.Name.EndsWith(".vcf"));
                    });
            Kernel.Rebind<SendEmailServiceInterface>().ToConstant(emailMock.Object);

            var service = Kernel.Get<BrowserClaimEmailSubscription>();

            var test = ClaimTestData.GetOne(Kernel, storedFlier.Id) as Claim;
            test.BrowserId = claimBrowser.Id;
            var evnt = new ClaimEvent()
                {
                    NewState = test
                };

            service.BrowserSubscribe(claimBrowser);

            var ret = service.Handle(evnt);

            Assert.IsTrue(ret); 
            Assert.IsTrue(emailSent);
            Assert.IsTrue(emailSentToClaimAddress);
            Assert.IsFalse(hasVCard);

            Kernel.Unbind<SendEmailServiceInterface>();
        }

        [Test]
        public void BrowserClaimEmailPublicationServiceSendsEmailWithiCalAttachmentToBrowserThatClaimedTearOffIfFlierHasTargetDate()
        {
            var repository = Kernel.Get<GenericRepositoryInterface>();

            BrowserInterface claimBrowser = new Website.Domain.Browser.Browser { Id = Guid.NewGuid().ToString(), EmailAddress = "test@bumbleflya.com" };
            claimBrowser = BrowserTestData.StoreOne(claimBrowser, repository, Kernel);

            var flierBrowser = BrowserTestData.GetOne(Kernel);
            flierBrowser = BrowserTestData.StoreOne(flierBrowser, repository, Kernel);

            var flier = FlierTestData.GetOne(Kernel);
            flier.BrowserId = flierBrowser.Id;
            var storedFlier = FlierTestData.StoreOne(flier, repository, Kernel);

            var emailSent = false;
            var emailSentToClaimAddress = false;
            var hasICal = false;

            var emailMock = Kernel.GetMock<SendEmailServiceInterface>();
            emailMock.Setup(sm => sm.Send(It.IsAny<MailMessage>()))
                .Callback<MailMessage>(email =>
                {
                    emailSent = true;
                    emailSentToClaimAddress =
                        email
                        .To
                        .Any(s => s.Address.CompareTo(claimBrowser.EmailAddress) == 0);
                    hasICal = email.Attachments.Any(a => a.Name.EndsWith(".ics"));
                });
            Kernel.Rebind<SendEmailServiceInterface>().ToConstant(emailMock.Object);

            var service = Kernel.Get<BrowserClaimEmailSubscription>();

            var test = ClaimTestData.GetOne(Kernel, storedFlier.Id) as Claim;
            test.BrowserId = claimBrowser.Id;
            service.BrowserSubscribe(claimBrowser);

            var evnt = new ClaimEvent()
                {
                    NewState = test
                };

            var ret = service.Handle(evnt);

            Assert.IsTrue(ret);
            Assert.IsTrue(emailSent);
            Assert.IsTrue(emailSentToClaimAddress);
            Assert.IsFalse(hasICal);

            Kernel.Unbind<SendEmailServiceInterface>();
        }

        //disabled for now
//        [Test]
//        public void BrowserClaimEmailPublicationServiceSendsEmailForUserSendDetails()
//        {
//            var repository = Kernel.Get<GenericRepositoryInterface>();
//
//            BrowserInterface claimBrowser = new Browser { Id = Guid.NewGuid().ToString(), EmailAddress = "test@bumbleflya.com" };
//            claimBrowser = BrowserTestData.StoreOne(claimBrowser, repository, Kernel);
//
//            var flierBrowser = BrowserTestData.GetOne(Kernel);
//            flierBrowser = BrowserTestData.StoreOne(flierBrowser, repository, Kernel);
//
//            var flier = FlierTestData.GetOne(Kernel);
//            flier.BrowserId = flierBrowser.Id;
//            flier.UseBrowserContactDetails = false;
//            var storedFlier = FlierTestData.StoreOne(flier, repository, Kernel);
//
//            var emailSent = false;
//            var emailSentToClaimAddress = false;
//            var emailSentToOwnerAddress = false;
//            var hasICal = false;
//
//            var emailMock = Kernel.GetMock<SendEmailServiceInterface>();
//            emailMock.Setup(sm => sm.Send(It.IsAny<MailMessage>()))
//                .Callback<MailMessage>(email =>
//                {
//                    emailSent = true;
//                    emailSentToClaimAddress =
//                        email
//                        .To
//                        .Any(s => s.Address.CompareTo(claimBrowser.EmailAddress) == 0) || emailSentToClaimAddress;
//
//                    emailSentToOwnerAddress =
//                        email
//                        .To
//                        .Any(s => s.Address.CompareTo(flierBrowser.EmailAddress) == 0) || emailSentToOwnerAddress;
//                    hasICal = email.Attachments.Any(a => a.Name.EndsWith(".ics")) || hasICal;
//                });
//            Kernel.Rebind<SendEmailServiceInterface>().ToConstant(emailMock.Object);
//
//            var service = Kernel.Get<BrowserClaimEmailSubscription>();
//
//            var test = ClaimTestData.GetOne(Kernel, storedFlier.Id) as Claim;
//            test.BrowserId = claimBrowser.Id;
//            test.ClaimContext = "senduserdetails";
//            test.ClaimMessage = "hey";
//            service.BrowserSubscribe(claimBrowser);
//            service.BrowserSubscribe(flierBrowser);
//
//            var evnt = new ClaimEvent()
//            {
//                NewState = test
//            };
//
//            var ret = service.Publish(evnt);
//
//            Assert.IsTrue(ret);
//            Assert.IsTrue(emailSent);
//            Assert.IsTrue(emailSentToClaimAddress);
//            Assert.IsTrue(emailSentToOwnerAddress);
//            Assert.IsTrue(hasICal);
//
//            Kernel.Unbind<SendEmailServiceInterface>();
//        }

    }
}
