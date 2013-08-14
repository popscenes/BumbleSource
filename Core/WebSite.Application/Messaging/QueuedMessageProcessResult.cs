namespace Website.Application.Messaging
{
    public enum QueuedMessageProcessResult
    {
        Successful,
        Error,
        Retry,
        RetryError
    }
}
