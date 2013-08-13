using System;

namespace Website.Application.Messaging
{
    [Serializable]
    public class DataBusRedirectToStorage
    {
        public string StorageId { get; set; }

    }
}