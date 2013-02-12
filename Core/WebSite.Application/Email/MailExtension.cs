using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using Website.Application.Email.ICalendar;

namespace Website.Application.Email
{
    public static class MailExtension
    {
        /// <summary>
        /// Extension method to add an embedded calendar to a MailMessage
        /// </summary>
        /// <param name="message">Mail message</param>
        /// <param name="calendar">Calendar</param>
        /// <param name="attachmentName">name for part</param>
        public static void AddCalendar(this MailMessage message, Calendar calendar)
        {
            //"text/calendar; method=REQUEST;charset=\"utf-8\""

            var content = new System.Net.Mime.ContentType("text/calendar");
            content.Parameters.Add("method", "REQUEST");
            content.Parameters.Add("name", "meeting.ics");
            content.Parameters.Add("charset", "utf-8");
            var alt = AlternateView.CreateAlternateViewFromString(calendar.ToString(),content);
            message.AlternateViews.Add(alt);
        }

        /// <summary>
        /// Extension method to add a calendar as attachment to a MailMessage
        /// </summary>
        /// <param name="message">Mail message</param>
        /// <param name="calendar">Calendar</param>
        /// <param name="attachmentName"> name to call attachment </param>
        public static void AddCalendarAsAttachment(this MailMessage message, Calendar calendar, string attachmentName = null)
        {
            if (attachmentName == null)
                attachmentName = "calendarentry.ics";

            var attach = Attachment.CreateAttachmentFromString(calendar.ToString(), attachmentName, Encoding.UTF8,
                            "text/calendar");
            //new Attachment(calendar.GetCalendarContentStream(), "meeting.ics", "text/calendar")
            message.Attachments.Add(attach);
        }

        /// <summary>
        /// Extension method to add a embedded event to a MailMessage
        /// </summary>
        /// <param name="message">Mail message</param>
        /// <param name="calEvent">Event</param>
        public static void AddEvent(this MailMessage message, Event calEvent)
        {
            message.AddCalendar(CreateCalendar(calEvent));
        }

        /// <summary>
        /// Extension method to add an event as attachment to a MailMessage
        /// </summary>
        /// <param name="message">Mail message</param>
        /// <param name="calEvent">Event</param>
        public static void AddEventAsAttachment(this MailMessage message, Event calEvent)
        {
            message.AddCalendarAsAttachment(CreateCalendar(calEvent), GetValidFileName(calEvent.Title + ".ics"));
        }

        private static Calendar CreateCalendar(Event calEvent)
        {
            var calendar = new Calendar()
            {
                Version = "2.0",
                ProdID = "BumbleFlya//ICalendar 1.0 MIMEDIR//EN"
            };

            if (calEvent.Status == EventStatus.CANCELLED)
                calendar.CalendarMethod = CalendarMethod.CANCEL;

            calendar.Elements.Add(calEvent);

            return calendar;
        }

        public static void AddVCardAsAttachment(this MailMessage mailMessage, VCard.VCard vCard, string attachmentName = null)
        {
            var attach = Attachment.CreateAttachmentFromString(vCard.ToString(), GetValidFileName(vCard.GetFileName(attachmentName)), Encoding.UTF8,
                                        "text/vcard");
            mailMessage.Attachments.Add(attach);
        }

        public static void AddVCard(this MailMessage mailMessage, VCard.VCard vCard)
        {
            //var alt = AlternateView.CreateAlternateViewFromString(vCard.ToString(), new System.Net.Mime.ContentType("text/vcard; method=REQUEST;"));

            var alt = AlternateView.CreateAlternateViewFromString(vCard.ToString(), Encoding.UTF8, "text/vcard");

            mailMessage.AlternateViews.Add(alt);
        }

        private static string GetValidFileName(string fileName)
        {
            return Path.GetInvalidFileNameChars().Aggregate(fileName, (current, c) => current.Replace(c.ToString(), string.Empty));
        }


        public static void AddSimpleHtmlAlternate(this MailMessage message, string body)
        {
            const string html = "<html><body>{0}</body></html>";
            var content = string.Format(html, body);
            var alt = AlternateView.CreateAlternateViewFromString(content, Encoding.UTF8, "text/html");
            message.AlternateViews.Add(alt);
        }
    }
}
