using System;
using Website.Infrastructure.Command;

namespace Website.Domain.Content.Command
{
    [Serializable]
    public class SetImageStatusCommand : DefaultCommandBase
    {
        public string Id { get; set; }
        public ImageStatus Status { get; set; }
    }
}