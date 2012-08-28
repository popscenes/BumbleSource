using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Website.Application.Intergrations
{
    public class FaceBookEventVenue
    {
        public String street { get; set; }

        public String city { get; set; }

        public String country { get; set; }

        public float latitude { get; set; }

        public float longitude { get; set; }
    }
    public class FaceBookEvent
    {

        public String id { get; set; }

        public String name { get; set; }

        public String description { get; set; }

        public DateTime start_time { get; set; }

        public DateTime end_time { get; set; }

        public String location { get; set; }

        public String privacy { get; set; }

        public FaceBookEventVenue venue { get; set; }

        public string picture { get; set; }

    }
}
