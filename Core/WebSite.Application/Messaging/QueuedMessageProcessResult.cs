namespace Website.Application.Messaging
{
    public enum QueuedMessageProcessResult
    {
        Successful = 0,
        Error,
        Retry,
        RetryError
    }
}
