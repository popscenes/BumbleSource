using System;
using System.Net;
using System.Net.Mail;
using SendGridMail;
using SendGridMail.Transport;
using Website.Application.Command;
using Website.Application.Email.Command;
using Website.Application.Extension.Email;
using Website.Infrastructure.Configuration;

namespace Website.Application.Email
{
    public class SendGridSendMailImplementation : SendMailImplementationInterface
    {
        private readonly ConfigurationServiceInterface _config;

        public SendGridSendMailImplementation(ConfigurationServiceInterface config)
        {
            _config = config;
        }

        public QueuedCommandResult ProcessSendMailCommand(SendMailCommand mailCommand)
        {
            var normalMsg = mailCommand.MailMessage.ToMailMessage();
            //var msg = normalMsg.ToSendGridMessage();

            var creds = _config.GetSetting("SendGridCreds").Split(';');
            var credentials = new NetworkCredential(creds[0], creds[1]);

            var cli = new SmtpClient("smtp.sendgrid.com", 2525)
                {
                    Credentials = credentials,
                    DeliveryMethod = SmtpDeliveryMethod.Network
                };
            cli.Send(normalMsg);

            // Create an SMTP transport for sending email.
//            var transportWeb = SendGridMail.Transport.Web.GetInstance(credentials);
//            transportWeb.Deliver(msg);
//            var transportSmtp = SMTP.GetInstance(credentials, "smtp.sendgrid.com", 2525);
//            transportSmtp.Deliver(msg);
            return QueuedCommandResult.Successful;
        }
    }
}