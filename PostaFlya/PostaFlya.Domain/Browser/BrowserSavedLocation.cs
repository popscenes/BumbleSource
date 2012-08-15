using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PostaFlya.Domain.Browser
{
    public class BrowserSavedLocation
    {
        public Guid BrowserID { get; set; }
        public Location.Location Location { get; set; }
    }
}
