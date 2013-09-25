using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dark;
using Website.Infrastructure.Domain;

namespace Website.Azure.Common.Sql.Infrastructure
{
    internal class MappedPropertyVisitor
    {
        private readonly Dictionary<AggregateMapKey, TypeMapperValue> _typeMapper;


        public MappedPropertyVisitor(Dictionary<AggregateMapKey, TypeMapperValue> typeMapper)
        {
            _typeMapper = typeMapper;
        }

        public List<AggregateRootMemberTableInterface> Visit(object aggregateRoot)
        {
            var root = aggregateRoot as AggregateRootInterface;
            if (root == null)
                throw new ArgumentException("Must be IAggregateRootId");

            var ret = new List<AggregateRootMemberTableInterface>();

            var head = GetMappedPropertiesForAggregate(aggregateRoot.GetType());
            VisitInternal(root, root, head, ret);

            return ret;
        }

        private void VisitInternal(AggregateRootInterface aggregateRootIdRoot, object propertyIntance, MappedProperty propertyInfoNode, List<AggregateRootMemberTableInterface> ret)
        {
            TypeMapperValue mapperValue;
            if (_typeMapper.TryGetValue(AggregateMapKey.From(aggregateRootIdRoot.GetType(), propertyInfoNode.Property), out mapperValue))
            {
                if (propertyInfoNode.IsEnumerable)
                {
                    ret.AddRange(from object ele in (IEnumerable) propertyIntance 
                                 from AggregateRootMemberTableInterface tr in mapperValue.MapFrom(ele, aggregateRootIdRoot) 
                                 select tr);
                }
                else
                {
                    ret.AddRange(mapperValue.MapFrom(propertyIntance, aggregateRootIdRoot));
                }
                
            }

            if(propertyInfoNode.SubProperties != null)
                propertyInfoNode.SubProperties.ForEach(property =>
                        {
                            var prop = property.PropertyAccessor(propertyIntance);
                            VisitInternal(aggregateRootIdRoot, prop, property, ret);
                        });
        }

        private static Dictionary<Type, MappedProperty> _typeModelDefinitionMap = new Dictionary<Type, MappedProperty>();
        protected MappedProperty GetMappedPropertiesForAggregate(Type aggregateType)
        {
            MappedProperty mappedProperty;

            if (_typeModelDefinitionMap.TryGetValue(aggregateType, out mappedProperty))
                return mappedProperty;

            mappedProperty = new MappedProperty()
                                 {
                                     PropertyAccessor = o => o,
                                     SubProperties = new List<MappedProperty>(),
                                     SourceType = aggregateType
                                 };


            BuildPropertyMap(aggregateType, mappedProperty);

            InterlockedUtil.SafeDictionaryAdd(ref _typeModelDefinitionMap, aggregateType, mappedProperty);

            return mappedProperty;
        }

        private void BuildPropertyMap(Type aggregateType, MappedProperty mappedProperty)
        {
            var props = mappedProperty.SourceType.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).ToList();

            var properties = props
                .Where(info => !info.PropertyType.GetUnderlyingTypeIfNullable().IsScalar() &&
                               _typeMapper.ContainsKey(AggregateMapKey.From(aggregateType, info)))
                .Select(info => new MappedProperty()
                                    {
                                        Property = info,
                                        SourceType = info.PropertyType.GetUnderlyingType(),
                                        IsEnumerable = info.PropertyType.GetUnderlyingTypeEnumerable() != null,
                                        PropertyAccessor = info.GetPropertyGetterFn(),
                                    }).ToList();

            if (properties.Any(property => property.PropertyAccessor == null))
                throw new Exception("Property must be gettable");

            mappedProperty.SubProperties = properties;
            mappedProperty.SubProperties.ForEach(property => BuildPropertyMap(aggregateType, property));
        }

        internal class MappedProperty
        {
            public PropertyInfo Property { get; set; }
            public Type SourceType { get; set; }
            public Func<object, object> PropertyAccessor { get; set; }
            public bool IsEnumerable { get; set; }
            public List<MappedProperty> SubProperties { get; set; }
        }

    }


}