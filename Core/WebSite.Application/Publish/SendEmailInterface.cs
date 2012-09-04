using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Website.Application.Publish
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

    public interface SendEmailInterface
    {
        void Send(Email email);
    }
}
