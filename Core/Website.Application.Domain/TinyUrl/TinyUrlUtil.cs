using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Website.Application.Domain.TinyUrl
{
    public class TinyUrlUtil
    {

        public static string Increment(string start)
        {
            var current = InitCounter(start);
            for (var i = current.Length - 1; i >= 0; i--)
            {
                current[i] = (current[i] + 1) % UrlChars.Length;
                if (current[i] != 0)
                    break;
            }

            return current.Where(i => i >= 0)
                          .Select(i => UrlChars[i])
                          .Aggregate(new StringBuilder(), (builder, c) => builder.Append(c))
                          .ToString();
        }

        public static void Increment(TinyUrlRecord record)
        {
            var index = record.TinyUrl.LastIndexOf('/');
            var path = index >= 0 && index < record.TinyUrl.Length - 1 ? record.TinyUrl.Substring(index + 1) : "";
            var url = index >= 0 ? record.TinyUrl.Substring(0, index) : record.TinyUrl;
            path = Increment(path);
            record.TinyUrl = url + '/' + path;
        }

        private static readonly char[] UrlChars;

        static TinyUrlUtil()
        {
            var ret = new List<char>();
            for (var i = '0'; i <= '9'; i++)
            {
                ret.Add(i);
            }

            for (var i = 'a'; i <= 'z'; i++)
            {
                ret.Add(i);
            }

            UrlChars = ret.ToArray();
        }


        private static int[] InitCounter(string source)
        {
            source = '+' + source; //add bogus char to initialize first element to -1 for rollover space
            return source.Select(chr => Array.IndexOf<char>(UrlChars, chr)).ToArray();
        }

        
    }
}