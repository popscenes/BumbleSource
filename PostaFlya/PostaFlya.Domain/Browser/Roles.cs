using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace PostaFlya.Domain.Browser
{
    [Serializable]
    public class Roles : HashSet<string>
    {
        public Roles()
        {
        }

        public Roles(IEnumerable<string> collection)
            : base(collection)
        {
        }

        public Roles(string tagsString)
            : base(tagsString.Split())
        {
        }

        //because HashSet serialization constructor is private 
        public Roles(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            foreach (var tag in this)
            {
                if (stringBuilder.Length > 0)
                    stringBuilder.Append(' ');
                stringBuilder.Append(tag);

            }
            return stringBuilder.ToString();
        }

        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
                return true;

            var other = obj as Roles;
            return other != null && SetEquals(other);
        }

        public override int GetHashCode()
        {
            return this.Aggregate(0, (current, element) => current ^ element.GetHashCode());
        }
    }
}
