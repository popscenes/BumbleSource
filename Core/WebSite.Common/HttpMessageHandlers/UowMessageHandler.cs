using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Website.Infrastructure.Command;

namespace Website.Common.HttpMessageHandlers
{
    public class UowMessageHandler : DelegatingHandler
    {
        public UowMessageHandler()
        {
            
        }

        protected async override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var uow = request.Resolve<UnitOfWorkInterface>();

            uow.Begin();
            // Call the inner handler.
            var response = await base.SendAsync(request, cancellationToken);

            uow.End();
            return response;
        }
    }
}