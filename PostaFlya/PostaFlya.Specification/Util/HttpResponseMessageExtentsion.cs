using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using MbUnit.Framework;
using WebSite.Infrastructure.Command;

namespace PostaFlya.Specification.Util
{
    public static class HttpResponseMessageExtentsion
    {
        public static string EntityId(this HttpResponseMessage message)
        {
            if (message.Headers.Location == null)
                return null;
            return message.Headers.Location.PathAndQuery.Substring(
                message.Headers.Location.PathAndQuery.LastIndexOf("/", System.StringComparison.Ordinal) + 1);
        }

        public static void AssertStatusCode(this HttpResponseMessage message)
        {
            Assert.IsNotNull(message);          

            MsgResponse response;
            if(!message.IsSuccessStatusCode && message.TryGetContentValue(out response))
            {
                Assert.IsTrue(message.IsSuccessStatusCode, "Unsuccessful Status Code {0} message \n {1}", message.StatusCode, response.ToString());
            }
            else if (!message.IsSuccessStatusCode)
            {
                Assert.IsTrue(message.IsSuccessStatusCode, "Unsuccessful Status Code {0}", message.StatusCode);
            }
        }

        public static MsgResponse AssertStatusCodeFailed(this HttpResponseMessage message)
        {
            Assert.IsNotNull(message);

            MsgResponse response = null;
            message.TryGetContentValue(out response);
            if (response != null)
            {
                Assert.IsFalse(message.IsSuccessStatusCode, "Successful Status Code not expected {0} message \n {1}", message.StatusCode, response.ToString());
            }
            else
            {
                Assert.IsFalse(message.IsSuccessStatusCode, "Successful Status Code not expected {0}", message.StatusCode);
            }
            return response;
        }
    }
}
