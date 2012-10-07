using System.Runtime.Serialization;

namespace Website.Application.Email.ICalendar
{
   /// <summary>
   /// This helper defines the iCalendar object method associated with the calendar object.
   /// </summary>
   internal class CalendarMethodHelper
   {
      /// <summary>
      /// Convert Enum to a string
      /// </summary>
      public static string CalendarMethod(CalendarMethod calendarMethod)
      {
         switch ( calendarMethod )
         {
            case ICalendar.CalendarMethod.CANCEL:
               return "CANCEL";

            case ICalendar.CalendarMethod.PUBLISH:
               return "PUBLISH";

            case ICalendar.CalendarMethod.REQUEST:
            default:
               return "REQUEST";
         }
      }
   }

   /// <summary>
   /// Defines the iCalendar object method associated with the calendar object.
   /// </summary>
   [DataContract]
   public enum CalendarMethod
   {
      /// <summary>
      /// Request
      /// </summary>
      [EnumMember]
      REQUEST,
      /// <summary>
      /// Publish
      /// </summary>
      [EnumMember]
      PUBLISH,
      /// <summary>
      /// Cancel
      /// </summary>
      [EnumMember]
      CANCEL,
   }
}
