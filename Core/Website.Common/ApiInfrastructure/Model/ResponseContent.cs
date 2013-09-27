using System;
using System.Runtime.Serialization;

namespace Website.Common.ApiInfrastructure.Model
{
    [Serializable]
    [DataContract]
    public class ResponseContent
    {
        public enum StatusEnum
        {
            Ok = 200,
            ValidationError = 400,
            Unauthorized = 401,
            NotFound = 404,
            AccountAlreadyExists = 6001
        }

        public ResponseContent() : this(StatusEnum.Ok)
        {
        }

        public ResponseContent(StatusEnum status)
        {
            Code = (int) status;
            Status = status.ToString().ToUpper();
            Messages = new string[0];
        }

        public ResponseContent(string message, StatusEnum status)
            : this(status)
        {
            Messages = new[]{message};
        }

        public ResponseContent(StatusEnum status, string messageFmt, params object[] args)
            : this(status)
        {
            Messages = new[] { string.Format(messageFmt, args) };
        }

        [DataMember]
        public int Code { get; set; }
        [DataMember]
        public string Status { get; set; }
        [DataMember]
        public string[] Messages { get; set; }

    }

    [Serializable]
    [DataContract]
    public class ResponseContent<TContent> : ResponseContent
    {
        public static ResponseContent<TContent> GetResponse(TContent data, StatusEnum status = StatusEnum.Ok)
        {
            return new ResponseContent<TContent>(data, status);
        }

        public ResponseContent()
        {
;
        }

        public ResponseContent(TContent data)
        {
            Data = data;
        }

        public ResponseContent(TContent data, StatusEnum status) : base(status)
        {
            Data = data;
        }

        [DataMember]
        public TContent Data { get; set; }
    }
}