using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Dark;
using Newtonsoft.Json;
using Website.Infrastructure.Domain;

namespace Website.Azure.Common.Sql.Infrastructure
{
    public static class AggregateMemberMap 
    {
        internal static Dictionary<AggregateMapKey, TypeMapperValue> TypeMap = new Dictionary<AggregateMapKey, TypeMapperValue>();
        internal static Dictionary<Type, List<Type>> AggregateToMapTables = new Dictionary<Type, List<Type>>();



        public static void Clear()
        {
            InterlockedUtil.SafeDictionarySwap(ref TypeMap, new Dictionary<AggregateMapKey, TypeMapperValue>());
        }

        public static void AddMap<TAggregate, TFrom, TTo>(Expression<Func<TAggregate, TFrom>> forMember, Action<TAggregate, TFrom, TTo> mapp = null, Func<TAggregate, long> shardFunc = null)
            where TAggregate : class, AggregateRootInterface
            where TTo : class, AggregateRootMemberTableInterface, new()
        {
            Func<Func<TAggregate, TTo>, TAggregate, TFrom, IEnumerable<TTo>> mappTrans
                = (func, aggregate, source) =>
                      {
                          var target = func(aggregate);
                          if(mapp != null)
                              mapp(aggregate, source, target);
                          return new List<TTo>() {target};
                      };
            AddMap(forMember, mappTrans, shardFunc);
        }

        public static void AddMap<TAggregate, TFrom, TTo>(Expression<Func<TAggregate, TFrom>> forMember, Func<Func<TAggregate, TTo>, TAggregate, TFrom, IEnumerable<TTo>> mapp, Func<TAggregate, long> shardFunc = null)
            where TAggregate : class, AggregateRootInterface
            where TTo : class, AggregateRootMemberTableInterface, new()
        {

            var factory = GetAggregateMapCreator<TAggregate, TTo>();
            var value = new TypeMapperValue
                            {
                                TableRowFactory =
                                    (aggregateRoot, source) =>
                                    mapp(factory, aggregateRoot as TAggregate, (TFrom)source),
                                TargetType = typeof (TTo)
                            };

            var forProperty = forMember.GetPropertyInfoFor();
            if(forProperty == null && (typeof(TAggregate) != typeof(TFrom)))
                throw new ArgumentException("Unable to determine property to use for mapping");

            var key = AggregateMapKey.From(typeof(TAggregate), forProperty);
            InterlockedUtil.SafeDictionaryAdd(ref TypeMap, key, value);
        }

        private static Func<TAggregate, TTo> GetAggregateMapCreator<TAggregate, TTo>(Func<TAggregate, long> shardFunc = null) 
            where TAggregate : class
            where TTo : class, AggregateRootMemberTableInterface, new()
        {
            var isRootStorage = (typeof(AggregateRootTable).IsAssignableFrom(typeof(TTo)));
            if (isRootStorage)
            {
                var initializerDelegate = typeof(TTo).CreateObjectInitDelegate("Id", "FriendlyId", "Json", "ClrType", "JsonHash", "ShardId")
                                          as Func<string, string, string, string, long, long, TTo>;

                return
                    agg =>
                        {
                            var json = JsonConvert.SerializeObject(agg);
                            return initializerDelegate(
                                (agg as AggregateRootInterface).Id,
                                (agg as AggregateRootInterface).FriendlyId,
                                json,
                                agg.GetType().GetAssemblyQualifiedNameWithoutVer(),
                                (uint)json.GetHashCode(),
                                (shardFunc != null ? shardFunc(agg) : 0L)
                                );
                        };
            }
            else
            {
                var initializerDelegate = typeof(TTo).CreateObjectInitDelegate("Id", "FriendlyId", "ShardId")
                                          as Func<string, string, long, TTo>;
                return
                    agg =>
                        {
                            return initializerDelegate(
                                (agg as AggregateRootInterface).Id,
                                (agg as AggregateRootInterface).FriendlyId,
                                shardFunc != null ? shardFunc(agg as TAggregate) : 0L
                                );
                        };

            }
        }

        internal static List<Type> GetRowTypesForDelete(this AggregateRootInterface aggregateRootId)
        {
            List<Type> types;
            if (!AggregateToMapTables.TryGetValue(aggregateRootId.GetType(), out types))
            {
                types =  TypeMap.Where(pair => pair.Key.Aggregate.IsInstanceOfType(aggregateRootId)
                                               && pair.Key.MapProperty != null).Select(
                                                   pair => pair.Value.TargetType).ToList();
                InterlockedUtil.SafeDictionaryAdd(ref AggregateToMapTables, aggregateRootId.GetType(), types);
            }
            return types;
        }



        internal static List<Type> GetTypesFromMaps()
        {
            return TypeMap.Select(pair => pair.Value.TargetType).Distinct().ToList();
        }

        internal static Type GetEntityStorageTypeForAggregate<TAggregate>() where TAggregate : class, AggregateRootInterface
        {
            return GetEntityStorageTypeForAggregate(typeof(TAggregate));
        }

        internal static Type GetEntityStorageTypeForAggregate(Type type)
        {
            var key = AggregateMapKey.From(type, null);
            return TypeMap[key].TargetType;
        }


        internal static Dictionary<AggregateMapKey, TypeMapperValue> Map { get { return TypeMap; } }
    }

    internal class TypeMapperValue
    {
        public Func<AggregateRootInterface, object, IEnumerable<AggregateRootMemberTableInterface>> TableRowFactory { get; set; }
        public Type TargetType { get; set; }

        public IEnumerable<AggregateRootMemberTableInterface> MapFrom(object source, AggregateRootInterface aggregateRootId)
        {
            return TableRowFactory(aggregateRootId, source);
        }
    }

    internal class AggregateMapKey
    {
        public Type Aggregate { get; set; }
        public PropertyInfo MapProperty { get; set; }

        public static AggregateMapKey From(Type aggregate, PropertyInfo source)
        {
            return new AggregateMapKey() { Aggregate = aggregate, MapProperty = source };
        }
        protected bool Equals(AggregateMapKey other)
        {
            return Aggregate == other.Aggregate && MapProperty == other.MapProperty;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((AggregateMapKey)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return MapProperty == null 
                    ? Aggregate.GetHashCode() 
                    : ((Aggregate.GetHashCode() * 397) ^ (MapProperty.GetHashCode()));
            }
        }

        public static bool operator ==(AggregateMapKey left, AggregateMapKey right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(AggregateMapKey left, AggregateMapKey right)
        {
            return !Equals(left, right);
        }
    }
}