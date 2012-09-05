using System.Collections.Generic;

namespace Website.Infrastructure.Service
{
    public class Email
    {
        public string Title { get; set; }
        public string Recipients { get; set; }
        public string Body { get; set; }
        public List<EmailAttachment> Attachments { get; set; } 
    }

    public class EmailAttachment
    {
        public string FileName { get; set; }
        public byte [] Content { get; set; }
    }

    public interface SendEmailServiceInterface
    {
        void Send(Email email);
    }
}
