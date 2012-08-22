using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace WebSite.Infrastructure.Domain
{
    [Serializable]
    public class PropertyGroupCollection 
    {
        public PropertyGroupCollection()
        {
            
        }

        public PropertyGroupCollection(IEnumerable<PropertyGroup> source)
        {
            if(source != null)
            {
                _propertyGroups = new Dictionary<string, PropertyGroup>(
                    source.Select(pg => new PropertyGroup(pg)).ToDictionary(p => p.Name)
                    );
            } 
        }

        public PropertyGroup[]  Groups
        {
            get { return this; }
            set
            {
                _propertyGroups.Clear();
                foreach (var propertyGroup in value)
                {
                    Add(propertyGroup);
                }
            }
        }

        public static implicit operator PropertyGroupCollection(PropertyGroup[] source)
        {
            var d = new PropertyGroupCollection(source);
            return d;
        }

        static public implicit operator PropertyGroup[](PropertyGroupCollection collection)
        {
            return collection._propertyGroups.Values.ToArray();
        }

        private readonly IDictionary<string, PropertyGroup> _propertyGroups = new Dictionary<string, PropertyGroup>();
        public void Add(PropertyGroup group)
        {
            _propertyGroups.Add(group.Name, group);
        }

        public PropertyGroup this[string groupKey]
        {
            get
            {
                PropertyGroup ret = null;
                this._propertyGroups.TryGetValue(groupKey, out ret);
                return ret;
            }
            set
            {
                this._propertyGroups[groupKey] = value;
            }
        }

        public object this[string group, string key]
        {
            get
            {
                PropertyGroup ret = null;
                this._propertyGroups.TryGetValue(group, out ret);
                return ret == null ? null : ret[key];
            }
            set
            {
                PropertyGroup ret = null;
                this._propertyGroups.TryGetValue(group, out ret);
                if (ret == null)
                {
                    ret = new PropertyGroup(){Name = group};
                    this._propertyGroups[group] = ret;
                }
                ret[key] = value;
            }
        }

//        public IEnumerator<PropertyGroup> GetEnumerator()
//        {
//            return _propertyGroups.Select(kv => kv.Value).GetEnumerator();
//        }
//
//        IEnumerator IEnumerable.GetEnumerator()
//        {
//            return GetEnumerator();
//        }
    }
}