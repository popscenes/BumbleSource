using System;

namespace Website.Application.Command
{
    [Serializable]
    public class DataBusRedirectToStorage
    {
        public string StorageId { get; set; }

    }
}