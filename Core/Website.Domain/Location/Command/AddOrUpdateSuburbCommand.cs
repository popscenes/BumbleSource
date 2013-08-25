using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Website.Infrastructure.Command;

namespace Website.Domain.Location.Command
{
    [Serializable]
    public class AddOrUpdateSuburbCommand : DefaultCommandBase
    {
        public Suburb Update { get; set; }
    }
}
