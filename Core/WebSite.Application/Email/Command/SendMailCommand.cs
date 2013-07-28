using System;
using Website.Infrastructure.Command;

namespace Website.Application.Email.Command
{
    [Serializable]
    public class SendMailCommand : CommandInterface
    {
        public SerializableMailMessage MailMessage { get; set; }

        #region Implementation of CommandInterface

        public string MessageId { get; set; }

        #endregion
    }
}