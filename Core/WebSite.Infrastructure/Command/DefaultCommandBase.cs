using System;

namespace Website.Infrastructure.Command
{
    [Serializable]
    public abstract class DefaultCommandBase : CommandInterface
    {
        protected DefaultCommandBase()
        {
            MessageId = Guid.NewGuid().ToString();
        }
        #region Implementation of CommandInterface

        public string MessageId { get; set; }

        #endregion
    }
}
