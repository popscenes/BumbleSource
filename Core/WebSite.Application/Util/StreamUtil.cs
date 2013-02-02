using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace Website.Application.Util
{
    public static class StreamUtil
    {
        public static string GetToString(Stream source, string encodingString)
        {
            var bytes = new byte[source.Length];
            source.Read(bytes, 0, bytes.Length);

            try
            {
                if (string.IsNullOrWhiteSpace(encodingString))
                    encodingString = System.Text.Encoding.ASCII.BodyName;

		        var encoding = System.Text.Encoding.GetEncoding(encodingString);
                return encoding.GetString(bytes);
            }
	        catch (Exception)
	        {

	            return null;
	        }

            
        }
    }
}
