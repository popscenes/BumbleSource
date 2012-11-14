﻿using System;
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
        [Display(Name = "Location", ResourceType = typeof(Properties.Resources))] 
        public LocationModel Location { get; set; }

        [Display(Name = "ImageTitle", ResourceType = typeof(Properties.Resources))] 
        public string Title { get; set; }
    }
}