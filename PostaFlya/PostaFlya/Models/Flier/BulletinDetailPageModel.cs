using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PostaFlya.Areas.Default.Models;

namespace PostaFlya.Models.Flier
{
    public class BulletinDetailPageModel : PageModelInterface
    {
        public string PageId { get; set; }
        public string ActiveNav { get; set; }
        public DefaultDetailsViewModel Detail { get; set; }
    }
}