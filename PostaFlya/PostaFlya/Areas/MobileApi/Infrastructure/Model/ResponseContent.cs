namespace PostaFlya.Areas.MobileApi.Infrastructure.Model
{
    public class ResponseContent
    {
        public enum StatusEnum
        {
            Ok = 1000,
            Authorized = 1001,
            AccountCreated = 1002,
            ValidationError = 2000,
            NotFound = 5000,
            NotScheduled = 5001,
            Unauthorized = 6000,
            AccountAlreadyExists = 6001
        }

        public ResponseContent() : this(StatusEnum.Ok)
        {
        }

        public ResponseContent(StatusEnum status)
        {
            Code = (int) status;
            Status = status.ToString().ToUpper();
        }

        public ResponseContent(string message, StatusEnum status)
            : this(StatusEnum.Ok)
        {
            Messages = new[]{message};
        }

        public int Code { get; set; }
        public string Status { get; set; }
        public string[] Messages { get; set; }

    }

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

        public TContent Data { get; set; }
    }
}