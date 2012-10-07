using System.Collections.Generic;
using System.Net.Mail;

namespace Website.Application.Email
{
    public interface SendEmailServiceInterface
    {
        void Send(MailMessage email);
    }
}
