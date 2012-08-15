using System;

namespace WebSite.Application.Command
{
    [Serializable]
    public class DataBusRedirectToStorage
    {
        public string StorageId { get; set; }

    }
}