using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json;

namespace WebSite.Application.Util
{
    public static class WebRequestUtil
    {
        public static RetType GetFromJson<RetType>(string requestUrl)
        {
            var request = WebRequest.Create(requestUrl);
            return request.GetFromJson<RetType>();
        }


        public static RetType GetFromJson<RetType>(this WebRequest webRequest)
        {
            var request = webRequest as HttpWebRequest;
            if (request == null)
                throw new ArgumentException("expecting http request");

            request.ContentType = "application/json";
            request.Accept = "application/json";
            RetType retEntity;

            using (var response = request.GetResponse())
            {
                using (var responseStream = response.GetResponseStream())
                {

                    using (var reader = new StreamReader(responseStream))
                    {
                        var json = reader.ReadToEnd();
                        retEntity = JsonConvert.DeserializeObject<RetType>(json);
                    }

                }
            }

            return retEntity;
        }
    }
}
