using System;
using System.Web.Script.Serialization;
using System.Linq;

namespace PostaFlya.Domain.Tag
{
    [Serializable]
    public class TagGroup
    {
        public String ParentTag { get; set; }

        protected Tags _tags;

        public String[] TagsListArray { get { return _tags.ToArray(); } set { _tags = new Tags(String.Join(",", value)); } }

        [ScriptIgnore]
        public Tags TagsList { get { return _tags; } set { _tags = value; } } 
 
    }
}