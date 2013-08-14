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

        public object Handle(SendMailCommand command)
        {
            return _sendMailImplementation.ProcessSendMailCommand(command);
        }
    }
}