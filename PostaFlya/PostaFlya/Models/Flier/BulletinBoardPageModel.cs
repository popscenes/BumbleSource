using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PostaFlya.Models.Flier
{
    public class BulletinBoardPageModel : PageModelInterface
    {
        public BulletinBoardPageModel()
        {
            Fliers = new List<BulletinFlierModel>();
        }
        public string PageId { get; set; }
        public string ActiveNav { get; set; }

        public IList<BulletinFlierModel> Fliers { get; set; }
    }
}