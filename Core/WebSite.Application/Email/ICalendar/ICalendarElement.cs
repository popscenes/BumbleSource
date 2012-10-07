namespace Website.Application.Email.ICalendar
{
   /// <summary>
   /// Interface for a iCalendar component.
   /// </summary>
   public interface ICalendarElement
   {
      /// <summary>
      /// Return the class in RFC-2445 format
      /// </summary>
      string GetFormattedElement();

   }
}
