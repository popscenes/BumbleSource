using System;
using WebSite.Infrastructure.Command;

namespace Website.Domain.Command
{
    [Serializable]
    public abstract class DomainCommandBase : CommandInterface
    {
        protected DomainCommandBase()
        {
            CommandId = Guid.NewGuid().ToString();
        }
        #region Implementation of CommandInterface

        public string CommandId { get; set; }

        #endregion
    }
}
