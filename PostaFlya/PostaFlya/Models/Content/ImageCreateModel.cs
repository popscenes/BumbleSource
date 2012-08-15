using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using PostaFlya.Models.Location;

namespace PostaFlya.Models.Content
{
    public class ImageCreateModel
    {
        [DisplayName("Location")]
        public LocationModel Location { get; set; }

        [DisplayName("Title")]
        public string Title { get; set; }
    }
}