using System;
using System.Net.Mail;
using Website.Application.Binding;
using Website.Application.Email.Command;
using Website.Application.Extension.Email;
using Website.Infrastructure.Binding;
using Website.Infrastructure.Command;

namespace Website.Application.Email
{
    public class QueuedSendEmailService : SendEmailServiceInterface
    {
        private readonly CommandBusInterface _commandBus;

        public QueuedSendEmailService([WorkerCommandBus]CommandBusInterface commandBus)
        {
            _commandBus = commandBus;
        }

        public void Send(MailMessage email)
        {
            _commandBus
                .Send(
                    new SendMailCommand()
                        {
                            CommandId = Guid.NewGuid().ToString(), MailMessage = email.ToSerializableMessage()
                        });
        }
    }
}