using System;
using System.Linq;
using System.Web.Script.Serialization;

namespace Website.Domain.Tag
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