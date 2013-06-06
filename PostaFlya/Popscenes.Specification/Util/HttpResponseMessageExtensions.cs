using System.Diagnostics;
using System.IO;
using System.Net.Http;

namespace Popscenes.Specification.Util
{
    public static class HttpResponseMessageExtensions
    {
        public static StreamContent GetContentCopy(this HttpContent content, bool traceContent = false)
        {
            if (content == null)
                return null;

            // if the original content is an ObjectContent, then this particular CopyToAsync() call would cause the MediaTypeFormatters to 
            // take part in Serialization of the ObjectContent and the result of this serialization is stored in the provided target memory stream.
            var ms = new MemoryStream();
            content.CopyToAsync(ms).Wait();
            ms.Position = 0;

            if(traceContent)
            {
                try
                {
                    var sr = new StreamReader(ms);
                    var myStr = sr.ReadToEnd();
                    Trace.TraceInformation("Debug Json:" + myStr);
                }
                finally
                {
                    ms.Position = 0;
                }
            }


            var streamContent = new StreamContent(ms);
            foreach (var header in content.Headers)
            {
                streamContent.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            return streamContent;
        }
    }
}