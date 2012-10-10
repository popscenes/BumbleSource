using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Website.Application.Command;
using Website.Application.Email.Command;

namespace Website.Application.Email
{
    public interface SendMailImplementationInterface
    {
        QueuedCommandResult ProcessSendMailCommand(SendMailCommand mailCommand);
    }

    public class SendGridSendMailImplementation : SendMailImplementationInterface
    {
        public QueuedCommandResult ProcessSendMailCommand(SendMailCommand mailCommand)
        {
            throw new NotImplementedException();
        }
    }
}
