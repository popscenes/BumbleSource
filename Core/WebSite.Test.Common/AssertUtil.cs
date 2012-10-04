using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Website.Test.Common
{
    public static class AssertUtil
    {
        public static void Count(int expected, IEnumerable enumerable)
        {
            Assert.That(enumerable.OfType<object>().Count(), Is.EqualTo(expected));
        }
        public static void AreEquivalent<ItemType>(IEnumerable<ItemType> actual, IEnumerable<ItemType> expected, Func<ItemType, ItemType, bool> customComp )

        {
            var expectedList = expected as List<ItemType> ?? expected.ToList();
            var actualList = actual as List<ItemType> ?? actual.ToList();
            Assert.That(expectedList.Count(), Is.EqualTo(actualList.Count()));

            var set = new HashSet<ItemType>();
            foreach (var keyValuePair in expectedList)
            {
                var find = actualList.FirstOrDefault(a => customComp(a, keyValuePair));
                if (find == null || find.Equals(default(ItemType)))
                {
                    Assert.Fail("Value not found {0}", keyValuePair);
                }
                set.Add(find);
            }

            Assert.That(set.Count(), Is.EqualTo(expectedList.Count()));
        }
        public static void AssertAreElementsEqualForKeyValPairsIncludeEnumerableValues<KeyType, ValType>(
            IEnumerable<KeyValuePair<KeyType, ValType>> expected
            , IEnumerable<KeyValuePair<KeyType, ValType>> actual)
        {
            
            //CollectionAssert.AreEquivalent doesn't have IComparer
            var expectedList = expected as List<KeyValuePair<KeyType, ValType>> ?? expected.ToList();
            var actualList = actual as List<KeyValuePair<KeyType, ValType>> ?? actual.ToList();
            Assert.That(expectedList.Count(), Is.EqualTo(actualList.Count()));

            var set = new HashSet<KeyValuePair<KeyType, ValType>>();
            foreach (var keyValuePair in expectedList)
            {
                var find = actualList.FirstOrDefault(a => EqKeyVal(a, keyValuePair));
                if(find.Equals(default(KeyValuePair<KeyType, ValType>)))
                {
                    Assert.Fail("Value not found {0}", keyValuePair);
                }
                set.Add(find);
            }

            Assert.That(set.Count(), Is.EqualTo(expectedList.Count()));
            //CollectionAssert.AreEquivalent(expected, actual, EqKeyVal); 
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

        public static void AreEqual(DateTime actual, DateTime expected, TimeSpan difference)
        {
            if (actual >= (expected - difference) && actual <= (expected + difference))
                return;
            throw new AssertionException("expected " + expected + "to be equal within " + difference + " of " + actual);
        }
    }
}
