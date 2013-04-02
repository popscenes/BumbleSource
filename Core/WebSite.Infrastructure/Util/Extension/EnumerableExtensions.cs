using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Website.Infrastructure.Util.Extension
{
    public static class EnumerableExtensions
    {
        public static HashSet<CollType> ToHashSet<CollType>(this IEnumerable<CollType> source)
        {
            return new HashSet<CollType>(source);
        }
    }
}
