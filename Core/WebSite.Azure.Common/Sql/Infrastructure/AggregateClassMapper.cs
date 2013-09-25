using System;
using DapperExtensions.Mapper;
using Dark;

namespace Website.Azure.Common.Sql.Infrastructure
{
    public class AggregateClassMapper<T> : ClassMapper<T> where T : class
    {
        public AggregateClassMapper()
        {
            Type type = typeof(T);

            if (typeof(AggregateRootMemberTableInterface).IsAssignableFrom(typeof(T)))
            {
                var rowid = typeof(T).GetProperty("RowId");
                Map(rowid).Key(KeyType.Identity);

                var id = typeof(T).GetProperty("Id");
                Map(id).Key(KeyType.Assigned);
            }

            Table(type.Name);
            AutoMap();
        }
    }
}