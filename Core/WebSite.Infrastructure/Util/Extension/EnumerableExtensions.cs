using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.FSharp.Collections;

namespace Website.Infrastructure.Util.Extension
{
    public static class EnumerableExtensions
    {
        public static HashSet<CollType> ToHashSet<CollType>(this IEnumerable<CollType> source)
        {

            return new HashSet<CollType>(source);
        }

        public static FSharpList<TType> ToFSharpList<TType>(this IEnumerable<TType> input)
        {
            return CreateFSharpList(input.ToList(), 0);
        }
         
        private static FSharpList<TType> CreateFSharpList<TType>(List<TType> input, int index)
        {
            if(index >= input.Count)
            {
                return FSharpList<TType>.Empty;
            }
            else
            {
                return FSharpList<TType>.Cons(input[index], CreateFSharpList(input, index + 1));
            }
        }
    }


}
