using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;

namespace WebSite.Application.Intergrations
{
    public class FacebookPermissionSet : Dictionary<string, bool>
    {
        
    }

//    public class FacebookPermissionSetConverter : JavaScriptConverter
//    {
//        public override object Deserialize(IDictionary<string, object> dictionary, Type type, JavaScriptSerializer serializer)
//        {
//            FacebookPermissionSet permSet = new FacebookPermissionSet();
//            foreach (var item in dictionary)
//            {
//                int intval = (int)item.Value;
//                bool boolvalue = intval == 1;
//
//                permSet.Add(item.Key, boolvalue);
//            }
//
//            return permSet;
//        }
//
//        public override IDictionary<string, object> Serialize(object obj, JavaScriptSerializer serializer)
//        {
//
//            throw new ApplicationException("de-serializable only");
//        }
//
//        public override IEnumerable<Type> SupportedTypes
//        {
//            // let the serializer know we can accept your "MyObject" type. 
//            get { return new Type[] { typeof(FacebookPermissionSet) }; }
//        }
//    }
}
