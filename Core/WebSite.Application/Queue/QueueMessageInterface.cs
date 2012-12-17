namespace Website.Application.Queue
{
    public interface QueueMessageInterface
    {
        byte[] Bytes { get; set; }
    }

    public class QueueMessage : QueueMessageInterface
    {
        public QueueMessage(){}
        public QueueMessage(byte[] bytes)
        {
            Bytes = bytes;
        }
        public byte[] Bytes { get; set; }
    }
}