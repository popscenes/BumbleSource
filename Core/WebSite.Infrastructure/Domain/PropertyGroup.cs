using System;
using System.Collections.Generic;

namespace WebSite.Infrastructure.Domain
{
    [Serializable]
    public class PropertyGroup
    {
        public string Name { get; set; }

        private readonly IDictionary<string, object> _properties = new Dictionary<string, object>();
        public PropertyGroup()
        {
            
        }

        public PropertyGroup(PropertyGroup source)
        {
            _properties = new Dictionary<string, object>(source.Properties);
            Name = source.Name;
        }

        public PropertyGroup(IEnumerable<KeyValuePair<string, object>> init)
        {
            foreach (var o in init)
            {
                _properties.Add(o);
            }
        }

        public IDictionary<string, object> Properties
        {
            get { return _properties; } 
        }

        public object this[string key]
        {
            get
            {
                object ret = null;
                this._properties.TryGetValue(key, out ret);
                return ret;
            }
            set
            {
                this._properties[key] = value;
            }
        }
    }
}
