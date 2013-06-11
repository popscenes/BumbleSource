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

        public JsonTableEntry(object sourceObject)
        {
            Init(sourceObject);
        }

        public JsonTableEntry()
        {
            
        }

        public string GetClrTyp()
        {
            return _clrTyp == null ? null : SerializeUtil.GetAssemblyQualifiedNameWithoutVer(_clrTyp);    
        }

        public void Init(object source)
        {
            _sourceObject = source;
            _clrTyp = source.GetType();
            UpdateEntry();
        }

        public void UpdateEntry()
        {
            _jsonText = JsonConvert.SerializeObject(_sourceObject);
        }

        public object GetEntity(Type entityTyp = null)
        {
            if (_sourceObject != null)
                return _sourceObject;

            if (_clrTyp != null && (entityTyp == null || entityTyp.IsAssignableFrom(_clrTyp)))
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