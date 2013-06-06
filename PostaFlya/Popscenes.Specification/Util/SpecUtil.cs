using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading;
using System.Web.Http;
using NUnit.Framework;
using Ninject;
using PostaFlya.Areas.MobileApi.Infrastructure.Model;
using TechTalk.SpecFlow;
using Website.Infrastructure.Command;
using Website.Infrastructure.Query;

namespace Popscenes.Specification.Util
{
    public static class SpecUtil
    {
        internal const string BaseAddressContext = "Address";


        private const string ServerContext = "Server"; 
        private const string KernelContext = "Kernel";
        private const string ResponseContext = "Response";
        private const string RequestContext = "Request";
        private const string RequestObjectContext = "RequestObjectContext";
        private const string RequestCookieContext = "RequestCookieContext";
        private const string ResponseCookieContext = "ResponseCookieContext";


        public static HttpServer Server
        {
            get { return ScenarioContext.Current.TryGet<HttpServer>(ServerContext); }
            set
            {
                var curr = Server;
                if(curr != null && curr != value)
                    curr.Dispose();

                ScenarioContext.Current[ServerContext] = value;
            }
        }

        public static CookieContainer RequestCookies
        {
            get { return ScenarioContext.Current.TryGet<CookieContainer>(RequestCookieContext); }
            set { ScenarioContext.Current[RequestCookieContext] = value; }
        }

        public static CookieContainer ResponseCookies
        {
            get { return ScenarioContext.Current.TryGet<CookieContainer>(ResponseCookieContext); }
            set { ScenarioContext.Current[ResponseCookieContext] = value; }
        }

        public static HttpResponseMessage ResponseMessage
        {
            get { return ScenarioContext.Current.TryGet<HttpResponseMessage>(ResponseContext); }
            set
            {
                var curr = ResponseMessage;
                if(curr != null && curr != value)
                    curr.Dispose();

                ScenarioContext.Current[ResponseContext] = value;
            }
        }

        public static HttpRequestMessage RequestMessage
        {
            get { return ScenarioContext.Current.TryGet<HttpRequestMessage>(RequestContext); }
            set
            {
                var curr = RequestMessage;
                if(curr != null && curr != value)
                    curr.Dispose();

                ScenarioContext.Current[RequestContext] = value;
            }
        }

        public static object RequestObject
        {
            get { return ScenarioContext.Current.TryGet<object>(RequestObjectContext); }
            set { ScenarioContext.Current[RequestObjectContext] = value; }
        }

        /// <summary>
        /// This gets a copy of the content as a certain type with out calling ReadAsAsync on the original content
        /// enables you to check the response as different types at runtime
        /// </summary>
        /// <typeparam name="TResponseContent"></typeparam>
        /// <returns></returns>
        public static TResponseContent GetResponseContentAs<TResponseContent>() where TResponseContent : ResponseContent
        {
            Assert.NotNull(ResponseMessage.Content);
            Assert.NotNull(ResponseMessage.Content.Headers.ContentType);
            Assert.That(ResponseMessage.Content.Headers.ContentType.ToString(), Contains.Substring(RequestMessage.Headers.Accept.First().MediaType));

            using(var contentCopy = ResponseMessage.Content.GetContentCopy())
            {
                return contentCopy.ReadAsAsync<TResponseContent>().Result;
            }
        }

        public static Uri GetUri(string path)
        {
            return new Uri(ScenarioContext.Current.TryGet<string>(BaseAddressContext) + path);
        }

        public static IKernel Kernel
        {
            get { return ScenarioContext.Current.TryGet<IKernel>(KernelContext); }
            set { ScenarioContext.Current[KernelContext] = value; }
        }

        public static TRet TryGet<TRet>(this ScenarioContext context, string keyName)
        {
            TRet ret;
            ScenarioContext.Current.TryGetValue<TRet>(keyName, out ret);
            return ret; 
        }

        public static object TryGet(this ScenarioContext context, string keyName)
        {
            return TryGet<object>(context, keyName);
        }


        public static void Request<TData>(string path, TData data, HttpMethod method)
        {
            HttpResponseMessage response;
            Request(path, data, method, out response);
            ResponseMessage = response;
        }

        public static void GetRequest(string path)
        {
            HttpResponseMessage response;
            GetRequest(path, out response);
            ResponseMessage = response;
        }

        public static void GetRequest(string path, out HttpResponseMessage response)
        {
            var request = new HttpRequestMessage { RequestUri = GetUri(path) };
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Method = HttpMethod.Get;
            
            Send(request, out response);
        }

        public static void Request(string path, object data, HttpMethod method, out HttpResponseMessage response)
        {
            var request = new HttpRequestMessage
                              {
                                  RequestUri = GetUri(path),
                                  Content = new ObjectContent(data.GetType(), data, new JsonMediaTypeFormatter())
                              };
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Method = method;

            Send(request, out response); 
        }

        public static void Send(HttpRequestMessage request, out HttpResponseMessage response)
        {
            RequestMessage = request;

            var cts = new CancellationTokenSource();
                       
            var handler = new CookieContainerDelegatingHandler(new InMemoryHttpContentSerializationHandler(Server))
                              {Cookies = RequestCookies};

            var messageInvoker = new HttpMessageInvoker(handler);
        
            response = messageInvoker.SendAsync(request, cts.Token).Result;

            ResponseCookies = handler.Cookies;
        }

        public static TReturn PerformQuery<TReturn, TQueryType>(TQueryType query) where TQueryType : QueryInterface
        {
            var ret = default(TReturn);

            var queryFactory = Kernel.Get<QueryChannelInterface>();
            ret = queryFactory.Query<TReturn, TQueryType>(query);
            
            return ret;
        }

    }


}
