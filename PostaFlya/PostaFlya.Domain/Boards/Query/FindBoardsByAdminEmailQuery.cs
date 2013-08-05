using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Website.Infrastructure.Query;

namespace PostaFlya.Domain.Boards.Query
{
    public class FindBoardsByAdminEmailQuery : QueryInterface<Board>
    {
        public String AdminEmail { get; set; }
    }
}
