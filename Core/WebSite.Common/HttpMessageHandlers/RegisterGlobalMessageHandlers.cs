using System.Web.Http;

namespace Website.Common.HttpMessageHandlers
{
    public static class RegisterGlobalMessageHandlers
    {
        public static void For(HttpConfiguration config)
        {
            config.MessageHandlers.Add(new UowMessageHandler());
        }
    }
}