﻿using System;
using System.Runtime.Serialization;

namespace Website.Application.Email.ICalendar
{
   /// <summary>
   /// Alarm before an event (example : "Before 15 min")
   /// </summary>
   [DataContract]
   public class BeforePeriodAlarm : Alarm
   {
      /// <summary>
      /// Before a Date
      /// </summary>
      [DataMember]
      public TimeSpan BeforeTime { get; set; }

      /// <summary>
      /// Format the trigger data
      /// </summary>
      protected override string GetTriggerString()
      {
         return ":-" + FormatHelper.FormatTimeSpan(this.BeforeTime);
      }
   }
}
