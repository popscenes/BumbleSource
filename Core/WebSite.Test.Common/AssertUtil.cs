using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MbUnit.Framework;

namespace WebSite.Test.Common
{
    public static class AssertUtil
    {
        public static void AssertAreElementsEqualForKeyValPairsIncludeEnumerableValues<KeyType, ValType>(
            IEnumerable<KeyValuePair<KeyType, ValType>> expected
            , IEnumerable<KeyValuePair<KeyType, ValType>> actual)
        {
            Assert.AreElementsEqualIgnoringOrder(expected, actual, EqKeyVal); 
        }

        private static bool EqKeyVal<KeyType, ValType>(KeyValuePair<KeyType, ValType> o
            , KeyValuePair<KeyType, ValType> o1)
        {
            if (o.Equals(o1)) return true;
            if (!o.Key.Equals(o1.Key)) return false;
            return EqCompareEnumerables(o.Value, o1.Value);
        }

        private static bool EqCompareEnumerables(object o, object o1)
        {
            if (o == null && o1 == null) return true;
            if (o == null || o1 == null) return false;
            if (o.Equals(o1)) return true;
            var oe = o as IEnumerable;
            var o1e = o1 as IEnumerable;
            if (oe == null || o1e == null) return false;
            IList<object> ol = oe.Cast<object>().ToList();
            IList<object> o1l = o1e.Cast<object>().ToList();
            if (ol.Count != o1l.Count) return false;
            return !ol.Where((t, i) => !EqCompareEnumerables(t, o1l[i])).Any();
        }

        public static void AssertAdjacentElementsAre<ElementType>(IEnumerable<ElementType> collection, Func<ElementType, ElementType, bool> comparer)
        {
            var list = collection.ToList();
            for (int i = 1; i < list.Count(); i++)
            {
                Assert.IsTrue(comparer(list[i - 1], list[i]));
            }
        }
    }
}
