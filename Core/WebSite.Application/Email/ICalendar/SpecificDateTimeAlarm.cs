using System;
using System.Runtime.Serialization;

namespace Website.Application.Email.ICalendar
{
   /// <summary>
   /// Alarm launched at a specific time
   /// </summary>
   [DataContract]
   public class SpecificDateTimeAlarm : Alarm
   {
      /// <summary>
      /// Specific DateTime for the alarm
      /// </summary>
      [DataMember]
      public DateTime SpecificDateTime { get; set; }

      /// <summary>
      /// Format the trigger data
      /// </summary>
      protected override string GetTriggerString()
      {
         return ";VALUE=DATE-TIME:" + this.SpecificDateTime.ToUniversalTime().ToString(FormatHelper.CAL_DATEFORMAT);
      }
   }
}
