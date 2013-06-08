using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Website.Infrastructure.Query
{
    public class FindByFriendlyIdQuery : QueryInterface
    {
        public string FriendlyId { get; set; }
    }
}
