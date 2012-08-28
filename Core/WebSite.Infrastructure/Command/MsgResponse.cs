using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Website.Infrastructure.Command
{
    public class MsgResponseDetail
    {
        public string Property { get; set; }
        public string Message { get; set; }
    }

    public class MsgResponse
    {
        public MsgResponse()
        {
        }

        public MsgResponse(string message, bool isError)
        {
            Message = message;
            IsError = isError;
        }

        public MsgResponse AddMessageProperty(string property, string value)
        {
            if(Details == null)
                Details = new List<MsgResponseDetail>();

            var ret = Details.SingleOrDefault(d => d.Property == property);
            if (ret != null)
                ret.Message = value;
            else
                Details.Add(new MsgResponseDetail() { Message = value, Property = property });

            return this;
        }

        public string Message { get; set; }
        public bool IsError { get; set; }
        public List<MsgResponseDetail> Details { get; set; }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append(Message);
            if(Details != null)
            foreach (var msgResponseDetail in Details)
            {
                builder.Append(',');
                builder.Append(msgResponseDetail.Property);
                builder.Append('=');
                builder.Append(msgResponseDetail.Message);
            }
            return builder.ToString();
        }
    }
}