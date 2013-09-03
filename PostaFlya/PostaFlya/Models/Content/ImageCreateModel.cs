using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using PostaFlya.Models.Location;

namespace PostaFlya.Models.Content
{
    public class ImageCreateModel
    {
        [Display(Name = "BoardCreateEditModel_Location", ResourceType = typeof(Properties.Resources))] 
        public LocationModel Location { get; set; }

        [Display(Name = "ImageTitle", ResourceType = typeof(Properties.Resources))] 
        public string Title { get; set; }

        public bool Anonymous { get; set; }

        public bool KeepFileImapeType { get; set; }
    }
}