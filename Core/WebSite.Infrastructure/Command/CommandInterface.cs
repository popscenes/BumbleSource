using System;

namespace Website.Infrastructure.Command
{
    public interface CommandInterface : MessageInterface 
    {
        
    }

//    [Serializable]
//    public abstract class DefaultCommandBase : CommandInterface
//    {
//        protected DefaultCommandBase()
//        {
//            CommandId = Guid.NewGuid().ToString();
//        }
//        #region Implementation of CommandInterface
//
//        public string CommandId { get; set; }
//
//        #endregion
//    }
}