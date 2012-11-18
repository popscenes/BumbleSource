using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PostaFlya.DataRepository.Search.SearchRecord;
using Website.Infrastructure.Command;

namespace PostaFlya.DataRepository.Search.Command
{
    [Serializable]
    public class ReindexFlierCommand : DefaultCommandBase
    {
        public FlierSearchRecord SearchRecord { get; set; }
        public bool UpdateOrDelete { get; set; }
    }
}
