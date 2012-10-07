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
            Kernel.Rebind<BrowserClaimEmailPublicationService>()
                .ToSelf().InTransientScope();
            Kernel.Rebind<CommandBusInterface>()
                .To<DefaultCommandBus>();
        }

        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
            Kernel.Unbind<BrowserClaimEmailPublicationService>();
            Kernel.Unbind<CommandBusInterface>();
            Kernel.Unbind <SendEmailServiceInterface>();
        }

        [Test]
        public void BrowserClaimEmailPublicationServiceSendsEmailWithVCardAttachmentToBrowserThatClaimedTearOffIfFlierHasContactDetails()
        {
            var repository = Kernel.Get<GenericRepositoryInterface>();
            
            BrowserInterface claimBrowser = new Browser {EmailAddress = "test@bumbleflya.com"};
            claimBrowser = BrowserTestData.StoreOne(claimBrowser, repository, Kernel);

            var flierBrowser = BrowserTestData.GetOne(Kernel);
            flierBrowser = BrowserTestData.StoreOne(flierBrowser, repository, Kernel);

            var flier = FlierTestData.GetOne(Kernel);
            flier.EffectiveDate = DateTime.UtcNow.AddDays(-1);
            flier.BrowserId = flierBrowser.Id;
            flier.UseBrowserContactDetails = true;
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

            var service = Kernel.Get<BrowserClaimEmailPublicationService>();

            var test = ClaimTestData.GetOne(Kernel, storedFlier.Id) as Claim;
            test.BrowserId = claimBrowser.Id;
            service.BrowserSubscribe(claimBrowser);

            var ret = service.Publish(test);

            Assert.IsTrue(ret); 
            Assert.IsTrue(emailSent);
            Assert.IsTrue(emailSentToClaimAddress);
            Assert.IsTrue(hasVCard);

            Kernel.Unbind<SendEmailServiceInterface>();
        }

        [Test]
        public void BrowserClaimEmailPublicationServiceSendsEmailWithiCalAttachmentToBrowserThatClaimedTearOffIfFlierHasTargetDate()
        {
            var repository = Kernel.Get<GenericRepositoryInterface>();

            BrowserInterface claimBrowser = new Browser { EmailAddress = "test@bumbleflya.com" };
            claimBrowser = BrowserTestData.StoreOne(claimBrowser, repository, Kernel);

            var flierBrowser = BrowserTestData.GetOne(Kernel);
            flierBrowser = BrowserTestData.StoreOne(flierBrowser, repository, Kernel);

            var flier = FlierTestData.GetOne(Kernel);
            flier.BrowserId = flierBrowser.Id;
            flier.UseBrowserContactDetails = false;
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

            var service = Kernel.Get<BrowserClaimEmailPublicationService>();

            var test = ClaimTestData.GetOne(Kernel, storedFlier.Id) as Claim;
            test.BrowserId = claimBrowser.Id;
            service.BrowserSubscribe(claimBrowser);

            var ret = service.Publish(test);

            Assert.IsTrue(ret);
            Assert.IsTrue(emailSent);
            Assert.IsTrue(emailSentToClaimAddress);
            Assert.IsTrue(hasICal);

            Kernel.Unbind<SendEmailServiceInterface>();
        }

    }
}
