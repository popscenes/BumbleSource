using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Website.Infrastructure.Query;

namespace PostaFlya.Domain.Flier.Query
{
    public class FindFlyersByBoardQuery : QueryInterface<Flier>
    {
        public string BoardId { get; set; }

        public DateTimeOffset Start { get; set; }

        public DateTimeOffset End { get; set; }
    }
}
