using System;
using System.Collections.Generic;
using Website.Infrastructure.Command;

namespace Website.Domain.Content.Command
{
    [Serializable]
    public class SetImageMetaDataCommand : DefaultCommandBase
    {
        public string Id { get; set; }
        public Location.Location Location { get; set; }
        public string Title { get; set; }
        public List<ImageDimension> Dimensions { get; set; }
    }
}
