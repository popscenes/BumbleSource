using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Website.Domain.Tag
{
    [Serializable]
    public class Tags : HashSet<string>
    {
        public Tags()
        {
        }

        public Tags(IEnumerable<string> collection)
            : base(collection)
        {
        }

        public Tags(string tagsString)
            : base((IEnumerable<string>) tagsString.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                     .Select(s => s.Trim()).Where(s => !string.IsNullOrWhiteSpace(s)))
        {
        }

        //because HashSet serialization constructor is private 
        public Tags(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            foreach (var tag in this)
            {
                if(stringBuilder.Length > 0)
                    stringBuilder.Append(",");
                stringBuilder.Append(tag);
                
            }
            return stringBuilder.ToString();
        }

        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
                return true;

            var other = obj as Tags;
            return other != null && SetEquals(other);
        }

        public override int GetHashCode()
        {
            return this.Aggregate(0, (current, element) => current ^ element.GetHashCode());
        }
    }
}