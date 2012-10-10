using Website.Application.Command;
using Website.Infrastructure.Command;

namespace Website.Application.Email.Command
{
    public class SendMailCommandHandler : CommandHandlerInterface<SendMailCommand>
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