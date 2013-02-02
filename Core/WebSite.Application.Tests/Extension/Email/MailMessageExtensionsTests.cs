using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Ninject.MockingKernel.Moq;
using Website.Application.Extension.Email;

namespace Website.Application.Tests.Extension.Email
{
    [TestFixture]
    public class MailMessageExtensionsTests
    {
        MoqMockingKernel Kernel
        {
            get { return TestFixtureSetup.CurrIocKernel; }
        }

        [Test]
        public void ToSendGridMessageHandlesAlternateUtf8TextViewTest()
        {

            const string html = "<html><body>This is a body</body></html>";
            const string text = "This is a body";
            var msg = new MailMessage
                {
                    IsBodyHtml = true,
                    Body = html,
                    From = new MailAddress("test@test.com")
                };

            msg.To.Add(new MailAddress("to@test.com"));
            msg.CC.Add(new MailAddress("cc@test.com"));
            msg.Bcc.Add(new MailAddress("bcc@test.com"));

            msg.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(text, System.Text.Encoding.UTF8, MediaTypeNames.Text.Plain));

            var msgRet = msg.ToSendGridMessage();
            Assert.That(msgRet.To.ElementAt(0).Address, Is.EqualTo("to@test.com"));
            Assert.That(msgRet.Cc.ElementAt(0).Address, Is.EqualTo("cc@test.com"));
            Assert.That(msgRet.Bcc.ElementAt(0).Address, Is.EqualTo("bcc@test.com"));

            Assert.That(msgRet.Text, Is.EqualTo(text));
            Assert.That(msgRet.Html, Is.EqualTo(html));

        }

        [Test]
        public void ToSendGridMessageHandlesAlternateAsciiTextViewTest()
        {
            const string html = "<html><body>This is a body</body></html>";
            const string text = "This is a body";
            var msg = new MailMessage
            {
                IsBodyHtml = true,
                Body = html,
                From = new MailAddress("test@test.com")
            };

            msg.To.Add(new MailAddress("to@test.com"));
            msg.CC.Add(new MailAddress("cc@test.com"));
            msg.Bcc.Add(new MailAddress("bcc@test.com"));

            msg.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(text, System.Text.Encoding.ASCII, MediaTypeNames.Text.Plain));

            var msgRet = msg.ToSendGridMessage();
            Assert.That(msgRet.To.ElementAt(0).Address, Is.EqualTo("to@test.com"));
            Assert.That(msgRet.Cc.ElementAt(0).Address, Is.EqualTo("cc@test.com"));
            Assert.That(msgRet.Bcc.ElementAt(0).Address, Is.EqualTo("bcc@test.com"));

            Assert.That(msgRet.Text, Is.EqualTo(text));
            Assert.That(msgRet.Html, Is.EqualTo(html));

        }

        [Test]
        public void ToSendGridMessageHandlesAlternateHtmlViewTest()
        {
            const string html = "<html><body>This is a body</body></html>";
            const string text = "This is a body";
            var msg = new MailMessage
            {
                IsBodyHtml = false,
                Body = text,
                From = new MailAddress("test@test.com")
            };

            msg.To.Add(new MailAddress("to@test.com"));
            msg.CC.Add(new MailAddress("cc@test.com"));
            msg.Bcc.Add(new MailAddress("bcc@test.com"));

            msg.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(html, System.Text.Encoding.UTF8, MediaTypeNames.Text.Html));

            var msgRet = msg.ToSendGridMessage();
            Assert.That(msgRet.To.ElementAt(0).Address, Is.EqualTo("to@test.com"));
            Assert.That(msgRet.Cc.ElementAt(0).Address, Is.EqualTo("cc@test.com"));
            Assert.That(msgRet.Bcc.ElementAt(0).Address, Is.EqualTo("bcc@test.com"));

            Assert.That(msgRet.Text, Is.EqualTo(text));
            Assert.That(msgRet.Html, Is.EqualTo(html));

        }

    }
}
