using System;
using Microsoft.WindowsAzure.Storage.Table.DataServices;
using Newtonsoft.Json;

namespace Website.Azure.Common.TableStorage
{
    public class JsonTableEntry : TableServiceEntity, StorageTableEntryInterface
    {
        public int PartitionClone { get; set; }

        public bool KeyChanged { get; set; }

        public void UpdateEntry(object source)
        {
            _sourceObject = source;
            _jsonText = JsonConvert.SerializeObject(source);
        }

        public object GetEntity(Type entityTyp)
        {
            return _sourceObject ?? (_sourceObject = JsonConvert.DeserializeObject(_jsonText, entityTyp));
        }

        public object GetSourceObject()
        {
            return _sourceObject;
        }

        //these aren't properties to stop them being serialized into table storage
        public string GetJson()
        {
            return _jsonText;
        }

        public void SetJson(string jsonText)
        {
            _sourceObject = null;
            _jsonText = jsonText;
        }

        private string _jsonText;
        private object _sourceObject;
    }
}