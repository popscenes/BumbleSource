using System;
using System.Collections.Generic;
using System.Net.Mail;
using Website.Application.Binding;
using Website.Infrastructure.Command;

namespace Website.Application.Email
{
    public interface SendEmailServiceInterface
    {
        void Send(MailMessage email);
    }


}
