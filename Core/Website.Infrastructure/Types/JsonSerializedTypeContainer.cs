using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Website.Infrastructure.Util;

namespace Website.Infrastructure.Types
{
    [Serializable]
    public class JsonSerializedTypeContainer
    {
        public string ClrTypeString { get; set; }
        public string JsonString { get; set; }
        public object GetObject()
        {
            return JsonConvert.DeserializeObject(JsonString, Type.GetType(ClrTypeString));
        }
        public static JsonSerializedTypeContainer Get(object source)
        {
            return new JsonSerializedTypeContainer()
                {
                    ClrTypeString = SerializeUtil.GetAssemblyQualifiedNameWithoutVer(source.GetType()),
                    JsonString = JsonConvert.SerializeObject(source)
                };
            
        }
    }
}
