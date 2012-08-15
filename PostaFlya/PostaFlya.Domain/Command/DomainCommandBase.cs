using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebSite.Infrastructure.Command;

namespace PostaFlya.Domain.Command
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
