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
            Fliers = new List<BulletinFlierSummaryModel>();
        }
        public string PageId { get; set; }
        public string ActiveNav { get; set; }

        public IList<BulletinFlierSummaryModel> Fliers { get; set; }
    }
}