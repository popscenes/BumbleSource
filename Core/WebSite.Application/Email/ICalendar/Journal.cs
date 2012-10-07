using System.Runtime.Serialization;

namespace Website.Application.Email.ICalendar
{
   [DataContract]
   internal class Journal : ICalendarElement
   {
      string ICalendarElement.GetFormattedElement()
      {
         return null;
      }
   }
}
