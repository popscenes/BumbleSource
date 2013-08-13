using Ninject;
using Ninject.Syntax;
using Website.Infrastructure.Messaging;

namespace Website.Infrastructure.Command
{
    public class DefaultMessageHandlerRepository : MessageHandlerRespositoryInterface
    {
        private readonly IResolutionRoot _resolver;
        public DefaultMessageHandlerRepository(IResolutionRoot resolver)
        {
            _resolver = resolver;
        }
        public MessageHandlerInterface<MessageType> FindHandler<MessageType>(MessageType command) where MessageType : MessageInterface
        {
            var res = _resolver.Get<MessageHandlerInterface<MessageType>>();
            return res;
        }
    }
}