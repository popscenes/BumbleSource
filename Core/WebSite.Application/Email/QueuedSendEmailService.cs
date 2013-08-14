using System;
using System.Net.Mail;
using Website.Application.Binding;
using Website.Application.Email.Command;
using Website.Application.Extension.Email;
using Website.Infrastructure.Binding;
using Website.Infrastructure.Command;
using Website.Infrastructure.Messaging;

namespace Website.Application.Email
{
    public class QueuedSendEmailService : SendEmailServiceInterface
    {
        private readonly MessageBusInterface _messageBus;

        public QueuedSendEmailService([WorkerCommandBus]MessageBusInterface messageBus)
        {
            _messageBus = messageBus;
        }

        public void Send(MailMessage email)
        {
            _messageBus
                .Send(
                    new SendMailCommand()
                        {
                            MessageId = Guid.NewGuid().ToString(), MailMessage = email.ToSerializableMessage()
                        });
        }
    }
}