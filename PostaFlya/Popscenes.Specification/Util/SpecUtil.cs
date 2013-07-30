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
using TechTalk.SpecFlow;
using Website.Common.ApiInfrastructure.Model;
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
        private const string AuthTokenContext = "AuthTokenContext";


        public static HttpServer Server
        {
            get { return ScenarioContext.Current.TryGet<HttpServer>(ServerContext); }
            set
            {
                var curr = Server;
                if(curr != null && curr != value)
                    curr.Dispose();

                if (value == null)
                    ScenarioContext.Current.Remove(ServerContext);
                else
                    ScenarioContext.Current[ServerContext] = value;
            }
        }

        public static CookieContainer RequestCookies
        {
            get { return ScenarioContext.Current.TryGet<CookieContainer>(RequestCookieContext); }
            set
            {
                if (value == null)
                    ScenarioContext.Current.Remove(RequestCookieContext);
                else
                    ScenarioContext.Current[RequestCookieContext] = value;
            }
        }

        public static CookieContainer ResponseCookies
        {
            get { return ScenarioContext.Current.TryGet<CookieContainer>(ResponseCookieContext); }
            set
            {
                if (value == null)
                    ScenarioContext.Current.Remove(ResponseCookieContext);
                else
                    ScenarioContext.Current[ResponseCookieContext] = value;
            }
        }

        public static HttpResponseMessage ResponseMessage
        {
            get { return ScenarioContext.Current.TryGet<HttpResponseMessage>(ResponseContext); }
            set
            {
                var curr = ResponseMessage;
                if(curr != null && curr != value)
                    curr.Dispose();

                if (value == null)
                    ScenarioContext.Current.Remove(ResponseContext);
                else
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

                if (value == null)
                    ScenarioContext.Current.Remove(RequestContext);
                else
                    ScenarioContext.Current[RequestContext] = value;
            }
        }

        public static string AuthToken
        {
            get { return ScenarioContext.Current.TryGet<string>(AuthTokenContext); }
            set
            {
                if (value == null)
                    ScenarioContext.Current.Remove(AuthTokenContext);
                else
                    ScenarioContext.Current[AuthTokenContext] = value;
            }
        }

        public static object RequestObject
        {
            get { return ScenarioContext.Current.TryGet<object>(RequestObjectContext); }
            set
            {
                if (value == null)
                    ScenarioContext.Current.Remove(RequestObjectContext);
                else
                    ScenarioContext.Current[RequestObjectContext] = value;
            }
        }


        public static ResponseContentType GetResponseContentAs<ResponseContentType>() where ResponseContentType : ResponseContent
        {
            Assert.NotNull(ResponseMessage.Content);
            Assert.NotNull(ResponseMessage.Content.Headers.ContentType);
            Assert.That(ResponseMessage.Content.Headers.ContentType.ToString(), Contains.Substring(RequestMessage.Headers.Accept.First().MediaType));

            using(var contentCopy = ResponseMessage.Content.GetContentCopy())
            {
                return contentCopy.ReadAsAsync<ResponseContentType>().Result;
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
            if(!string.IsNullOrWhiteSpace(AuthToken))
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", AuthToken);

            var cts = new CancellationTokenSource();
                       
            var handler = new CookieContainerDelegatingHandler(new InMemoryHttpContentSerializationHandler(Server))
                              {Cookies = RequestCookies};

            var messageInvoker = new HttpMessageInvoker(handler);
        
            response = messageInvoker.SendAsync(request, cts.Token).Result;

            ResponseCookies = handler.Cookies;
            if (ResponseCookies != null)
                RequestCookies = ResponseCookies;//set next request cookies
        }

        public static TReturn PerformQuery<TSource, TReturn>(QueryHandlerInterface<TSource, TReturn> query) where TSource 
            : class, QueryInterface
        {
            var ret = default(TReturn);
            PerformInUnitOfWork(() =>
            {

            });
            return ret;
        }

        public static void PerformInUnitOfWork(Action action)
        {

        }

        public static void AssertThatTableColsEqualModelProperties(TableRow row, object model)
        {
            //not going to traverse row key as expression here simple props, 
            //you have nested properties do the traversal yourself
            var type = model.GetType();
            var gotone = false;
            foreach (var col in row)
            {
                var prop =
                    type.GetProperties().SingleOrDefault(
                        info => info.Name.ToLowerInvariant() == col.Key.ToLowerInvariant());
                if (prop == null)
                {
                    Assert.Fail("Property Not Found" + col.Key);
                    continue;
                }
                gotone = true;
                var ob = prop.GetValue(model);
                Assert.That(col.Value, Is.EqualTo(ob.ToString()));
            }
            Assert.True(gotone);
        }
    }


}
