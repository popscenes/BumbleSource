using System;
using Website.Infrastructure.Command;

namespace Website.Application.Publish
{
    [Serializable]
    public class PublishCommand : DefaultCommandBase
    {
        public object PublishObject { get; set; }
    }
}