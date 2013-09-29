using System;
using System.Linq;
using DapperExtensions.Mapper;
using Dark;
using Website.Infrastructure.Sharding;
using Website.Infrastructure.Types;

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

            var fedProp = SerializeUtil.GetPropertyWithAttribute(type, typeof(FederationPropertyAttribute));
            if (fedProp != null)
            {
                var fedAtt = fedProp.GetCustomAttributes(true).First(a => a.GetType() == typeof(FederationPropertyAttribute)) as FederationPropertyAttribute;
                if (fedAtt != null && !fedAtt.IsReferenceTable)
                {
                    Map(fedProp).Key(KeyType.Assigned);
                }
            }


            Table(type.Name);
            AutoMap();
        }
    }
}