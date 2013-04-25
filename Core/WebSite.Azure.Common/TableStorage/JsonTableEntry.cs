using System;
using Microsoft.WindowsAzure.Storage.Table.DataServices;
using Newtonsoft.Json;
using Website.Infrastructure.Util;

namespace Website.Azure.Common.TableStorage
{
    public class JsonTableEntry : TableServiceEntity, StorageTableEntryInterface
    {
        public int PartitionClone { get; set; }

        public bool KeyChanged { get; set; }

        public string GetClrTyp()
        {
            return SerializeUtil.GetAssemblyQualifiedNameWithoutVer(_clrTyp);    
        }

        public void UpdateEntry(object source)
        {
            _sourceObject = source;
            _jsonText = JsonConvert.SerializeObject(source);
            _clrTyp = source.GetType();
        }

        public object GetEntity(Type entityTyp)
        {
            if (_sourceObject != null)
                return _sourceObject;

            if (_clrTyp != null && entityTyp.IsAssignableFrom(_clrTyp))
                _sourceObject = JsonConvert.DeserializeObject(_jsonText, _clrTyp);

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

        public void SetJson(string jsonText, string clrTyp)
        {
            _sourceObject = null;
            _jsonText = jsonText;
            if(!string.IsNullOrWhiteSpace(clrTyp))
                _clrTyp = Type.GetType(clrTyp, false);
        }

        private string _jsonText;
        private object _sourceObject;
        private Type _clrTyp;
    }
}