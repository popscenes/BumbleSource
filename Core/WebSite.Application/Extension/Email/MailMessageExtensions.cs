using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using SendGridMail;
using Website.Application.Email.Command;
using Website.Application.Util;

namespace Website.Application.Extension.Email
{
    public static class MailMessageExtensions
    {
        public static SendGrid ToSendGridMessage(this MailMessage source)
        {
            var ret = SendGrid.GetInstance();

            ret.To = source.To.ToArray();
            ret.Cc = source.CC.ToArray();
            ret.Bcc = source.Bcc.ToArray();
            ret.Subject = source.Subject;
            ret.From = source.From;

            var textAlt =
                source.AlternateViews.SingleOrDefault(
                    view => view.ContentType.ToString().Contains(MediaTypeNames.Text.Plain));

            var htmlAlt =
                source.AlternateViews.SingleOrDefault(
                    view => view.ContentType.ToString().Contains(MediaTypeNames.Text.Html));

            var text = textAlt != null ? StreamUtil.GetToString(textAlt.ContentStream, textAlt.ContentType.CharSet) : "";
            var html = htmlAlt != null ? StreamUtil.GetToString(htmlAlt.ContentStream, htmlAlt.ContentType.CharSet) : "";
            ret.Html = source.IsBodyHtml ? source.Body : html;
            ret.Text = !source.IsBodyHtml ? source.Body : text;

            foreach (var attach in source.Attachments)
            {
                ret.AddAttachment(attach.ContentStream, attach.Name);
            }

            return ret;
        }

        public static SerializableMailMessage ToSerializableMessage(this MailMessage mm)
        {
            var ret = new SerializableMailMessage
            {
                IsBodyHtml = mm.IsBodyHtml,
                Body = mm.Body,
                Subject = mm.Subject,
                From = SerializeableMailAddress.GetSerializeableMailAddress(mm.From),
                BodyEncoding = mm.BodyEncoding,
                DeliveryNotificationOptions = mm.DeliveryNotificationOptions,
                Headers = SerializeableCollection.GetSerializeableCollection(mm.Headers),
                Priority = mm.Priority,
                Sender = SerializeableMailAddress.GetSerializeableMailAddress(mm.Sender),
                SubjectEncoding = mm.SubjectEncoding
            };

            foreach (var ma in mm.To)
            {
                ret.To.Add(SerializeableMailAddress.GetSerializeableMailAddress(ma));
            }

            foreach (var ma in mm.CC)
            {
                ret.Cc.Add(SerializeableMailAddress.GetSerializeableMailAddress(ma));
            }

            foreach (var ma in mm.Bcc)
            {
                ret.Bcc.Add(SerializeableMailAddress.GetSerializeableMailAddress(ma));
            }

            foreach (var rt in mm.ReplyToList)
            {
                ret.ReplyTo.Add(SerializeableMailAddress.GetSerializeableMailAddress(rt));
            }

            foreach (Attachment att in mm.Attachments)
            {
                ret.Attachments.Add(SerializeableAttachment.GetSerializeableAttachment(att));
            }

            foreach (AlternateView av in mm.AlternateViews)
                ret.AlternateViews.Add(SerializeableAlternateView.GetSerializeableAlternateView(av));
            return ret;
        }

        public static MailMessage ToMailMessage(this SerializableMailMessage sm)
        {
            var mm = new MailMessage { IsBodyHtml = sm.IsBodyHtml, Body = sm.Body, Subject = sm.Subject };

            if (sm.From != null)
                mm.From = sm.From.GetMailAddress();

            foreach (var ma in sm.To)
            {
                mm.To.Add(ma.GetMailAddress());
            }

            foreach (var ma in sm.Cc)
            {
                mm.CC.Add(ma.GetMailAddress());
            }

            foreach (var ma in sm.Bcc)
            {
                mm.Bcc.Add(ma.GetMailAddress());
            }

            foreach (var rt in sm.ReplyTo)
            {
                mm.ReplyToList.Add(rt.GetMailAddress());
            }

            foreach (var att in sm.Attachments)
            {
                mm.Attachments.Add(att.GetAttachment());
            }

            mm.BodyEncoding = sm.BodyEncoding;
            mm.DeliveryNotificationOptions = sm.DeliveryNotificationOptions;
            sm.Headers.SetColletion(mm.Headers);
            mm.Priority = sm.Priority;

            if (sm.Sender != null)
                mm.Sender = sm.Sender.GetMailAddress();

            mm.SubjectEncoding = sm.SubjectEncoding;

            foreach (SerializeableAlternateView av in sm.AlternateViews)
                mm.AlternateViews.Add(av.GetAlternateView());

            return mm;
        }
    }
}