using System;
using System.Diagnostics;
using System.Net;

namespace Website.Application.Domain.Content
{
    public class ImageUrlContentRetriever : UrlContentRetrieverInterface
    {
        public Website.Domain.Content.Content GetContent(string url)
        {
            var ret = new Website.Domain.Content.Content { Type = Website.Domain.Content.Content.ContentType.Image };
            try
            {
                var webRequest = WebRequest.Create(url);
                using(var response = webRequest.GetResponse())
                {
                    var stream = response.GetResponseStream();
                    ret.Data = new byte[response.ContentLength];

                    var bytesRead = 0;
                    var totalBytesRead = bytesRead;
                    while (totalBytesRead < ret.Data.Length)
                    {
                        bytesRead = stream.Read(ret.Data, bytesRead, ret.Data.Length - bytesRead);
                        totalBytesRead += bytesRead;
                    }

                    stream.Close();
                }

            }
            catch (Exception e)
            {
                Trace.TraceError("WebImageRequestContentRetriever Error: {0}, Stack {1}", e.Message, e.StackTrace);
            }

            return ret;
        }
    }
}
