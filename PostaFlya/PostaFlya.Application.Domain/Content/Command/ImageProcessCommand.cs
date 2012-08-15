using System;
using WebSite.Infrastructure.Command;

namespace PostaFlya.Application.Domain.Content.Command
{
    [Serializable]
    public class ImageProcessCommand : CommandInterface
    {
        public byte[] ImageData { get; set; }

        #region Implementation of CommandInterface

        public string CommandId { get; set; }

        #endregion
    }
}