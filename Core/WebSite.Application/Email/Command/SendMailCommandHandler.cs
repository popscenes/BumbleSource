using Website.Infrastructure.Messaging;

namespace Website.Application.Email.Command
{
    public class SendMailCommandHandler : MessageHandlerInterface<SendMailCommand>
    {
        private readonly SendMailImplementationInterface _sendMailImplementation;

        public SendMailCommandHandler(SendMailImplementationInterface sendMailImplementation)
        {
            _sendMailImplementation = sendMailImplementation;
        }

        public void Handle(SendMailCommand command)
        {
            _sendMailImplementation.ProcessSendMailCommand(command);
        }
    }
}