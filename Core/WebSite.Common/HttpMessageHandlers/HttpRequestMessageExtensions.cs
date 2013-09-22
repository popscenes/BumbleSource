using System.Net.Http;

namespace Website.Common.HttpMessageHandlers
{
    public static class HttpRequestMessageExtensions
    {
        // service locatorishy antipatterny but message handlers don't play with DI atm
        public static ServiceType Resolve<ServiceType>(this HttpRequestMessage request) where ServiceType : class
        {
            return request.GetDependencyScope().GetService(typeof(ServiceType)) as ServiceType;
        }
    }
}