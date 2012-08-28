namespace Website.Application.Command
{
    public interface MessageFactoryInterface
    {
        QueueMessageInterface GetMessageForBytes(byte[] bytes);
    }
}