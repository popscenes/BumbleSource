using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PostaFlya.Domain.Command;

namespace PostaFlya.Domain.Content.Command
{
    public class SetImageMetaDataCommand : DomainCommandBase
    {
        public string Id { get; set; }
        public Location.Location Location { get; set; }
        public string Title { get; set; }
    }
}
