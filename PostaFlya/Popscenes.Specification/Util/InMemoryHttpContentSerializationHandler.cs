using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Popscenes.Specification.Util
{
    public class InMemoryHttpContentSerializationHandler : DelegatingHandler
    {
        public InMemoryHttpContentSerializationHandler()
        {
        }

        public InMemoryHttpContentSerializationHandler(HttpMessageHandler innerHandler)
            : base(innerHandler)
        {
        }

        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Replace the original content with a StreamContent before the request
            // passes through upper layers in the stack
            request.Content = ConvertToStreamContent(request.Content);

            var response = await base.SendAsync(request, cancellationToken);
            response.Content = ConvertToStreamContent(response.Content);
            return response;
        }

        private static StreamContent ConvertToStreamContent(HttpContent originalContent)
        {
            if (originalContent == null)
            {
                return null;
            }

            var streamContent = originalContent as StreamContent;

            return streamContent ?? originalContent.GetContentCopy(true);

        }

 
    }
}
