using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Website.Infrastructure.Binding
{
    public static class TypeExtension
    {
        public static IEnumerable<Type> GetInterfaceTypesForGeneric(this Type type, Type generic)
        {
            return type.GetInterfaces().Where(i =>
                                        i.IsGenericType &&
                                        i.GetGenericTypeDefinition() ==
                                        generic);
        }
    }
}
