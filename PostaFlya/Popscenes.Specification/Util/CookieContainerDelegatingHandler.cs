using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Popscenes.Specification.Util
{
    public class CookieContainerDelegatingHandler : DelegatingHandler
    {
        public CookieContainer Cookies { get; set; }

        public CookieContainerDelegatingHandler()
        {

        }

        public CookieContainerDelegatingHandler(HttpMessageHandler innerHandler)
            : base(innerHandler)
        {

        }

        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {

            if (Cookies != null && Cookies.Count > 0)
            {
                request.Headers.Remove("Cookie");
                request.Headers.Add("Cookie", Cookies.GetCookieHeader(request.RequestUri));
            }

            var response = await base.SendAsync(request, cancellationToken);

            IEnumerable<string> values;
            response.Headers.TryGetValues("Set-Cookie", out values);
            var setCookieHeaders = values != null ? values.ToList() : null;
            if(values == null || !setCookieHeaders.Any())
                return response;

            Cookies = new CookieContainer();
            foreach (var cookie in setCookieHeaders)
            {
                Cookies.SetCookies(request.RequestUri, cookie);
            }

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