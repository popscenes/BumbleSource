using System;
using System.Linq;
using AutoMapper;

namespace Website.Azure.Common.Sql.Infrastructure
{
    public static class AutoMapperExtensions
    {

        public static TResult MapToInstance<TResult>(this object source, TResult destination)
        {
            if (source == null)
                throw new ArgumentNullException();

            return (TResult)Mapper.Map(source, destination, source.GetType(), typeof(TResult));
        }

        public static TResult MapTo<TResult>(this object self)
        {
            if (self == null)
                throw new ArgumentNullException();

            return (TResult)Mapper.Map(self, self.GetType(), typeof(TResult));
        }


        public static IMappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>()
        {
            return Mapper.CreateMap<TSource, TDestination>().IgnoreAllNonExisting();
        }


        public static IMappingExpression<TSource, TDestination> IgnoreAllNonExisting<TSource, TDestination>(this IMappingExpression<TSource, TDestination> expression)
        {
            var sourceType = typeof(TSource);
            var destinationType = typeof(TDestination);
            var existingMaps = Mapper.GetAllTypeMaps().First(x => x.SourceType == sourceType
                                                                  && x.DestinationType == destinationType);
            foreach (var property in existingMaps.GetUnmappedPropertyNames())
            {
                expression.ForMember(property, opt => opt.Ignore());
            }
            return expression;
        }
    }
}