using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace PostaFlya.Domain.Location
{
    [Serializable]
    public class Locations : HashSet<Location>
    {
        public Locations()
        {
        }

        public Locations(IEnumerable<Location> collection)
            : base(collection)
        {
        }

        public Locations(string coordinateStrings)
            : base(coordinateStrings.Split().AsQueryable().Select(s => new Location(s)))
        {
        }

        //because HashSet serialization constructor is private 
        public Locations(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            foreach (var location in this)
            {
                if(stringBuilder.Length > 0)
                    stringBuilder.Append(' ');
                stringBuilder.Append(location);
                
            }
            return stringBuilder.ToString();
        }

        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
                return true;

            var other = obj as Locations;
            return other != null && SetEquals(other);
        }

        public override int GetHashCode()
        {
            return this.Aggregate(0, (current, element) => current ^ element.GetHashCode());
        }
    }
}
