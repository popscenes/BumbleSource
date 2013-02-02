using System;
using System.Net;
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
            var msg = mailCommand.MailMessage.ToMailMessage().ToSendGridMessage();

            var creds = _config.GetSetting("SendGridCreds").Split(';');
            var credentials = new NetworkCredential(creds[0], creds[1]);

            // Create an SMTP transport for sending email.
            var transportSmtp = SMTP.GetInstance(credentials);
            transportSmtp.Deliver(msg);
            return QueuedCommandResult.Successful;
        }
    }
}