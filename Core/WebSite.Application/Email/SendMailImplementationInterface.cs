using System.Collections.Generic;
using System.Linq;
using System.Text;
using Website.Application.Email.Command;
using Website.Application.Messaging;

namespace Website.Application.Email
{
    public interface SendMailImplementationInterface
    {
        QueuedMessageProcessResult ProcessSendMailCommand(SendMailCommand mailCommand);
    }
}
