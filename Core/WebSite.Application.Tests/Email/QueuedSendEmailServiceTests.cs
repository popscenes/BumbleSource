using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using Moq;
using NUnit.Framework;
using Ninject;
using Ninject.MockingKernel.Moq;
using Website.Application.Email;
using Website.Application.Email.Command;
using Website.Application.Email.VCard;
using Website.Application.Extension.Email;
using Website.Infrastructure.Command;
using Website.Infrastructure.Util;

namespace Website.Application.Tests.Email
{
    [TestFixture]
    public class QueuedSendEmailServiceTests
    {
        MoqMockingKernel Kernel
        {
            get { return TestFixtureSetup.CurrIocKernel; }
        }

        [TestFixtureSetUp]
        public void FixtureSetUp()
        {

        }

        [TestFixtureTearDown]
        public void FixtureTearDown()
        {

        }

        [Test]
        public void QueuedSendEmailServiceSendsSerializableMail()
        {
            var commandBus = Kernel.GetMock<CommandBusInterface>();
            commandBus.Setup(cb => cb.Send(It.IsAny<SendMailCommand>()))
                .Returns<SendMailCommand>(command =>
                    {
                        var commandSerial = SerializeUtil.ToByteArray(command);

                        var deserial = SerializeUtil.FromByteArray<SendMailCommand>(commandSerial);
                        var ret = deserial.MailMessage.ToMailMessage();
                        Assert.AreEqual(ret.From, new MailAddress("blah@blah.com"));
                        Assert.AreEqual(ret.To, new MailAddressCollection(){new MailAddress("blah2@blah.com")});
                        Assert.That(ret.Attachments.Count, Is.GreaterThan(0));

                        return true;
                    });
            Kernel.Bind<CommandBusInterface>().ToConstant(commandBus.Object);

            var sms = Kernel.Get<SendEmailServiceInterface>();
            var msg = new MailMessage("blah@blah.com", "blah2@blah.com");
            var test = new VCard {Email = "mycard@blah.com", FirstName = "Yo"};
            msg.AddVCardAsAttachment(test);
            msg.Body = "hello 123";

            sms.Send(msg);

            Kernel.Unbind<CommandBusInterface>();
        }

    }
}
