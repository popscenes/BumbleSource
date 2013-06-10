using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Website.Infrastructure.Util
{
    public static class TypeUtil
    {
        public static Type TryFindType(string typeName)
        {
            var type = Type.GetType(typeName);
            if (type != null) return type;
            foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = a.GetType(typeName);
                if (type != null)
                    return type;
            }
            return null;
        }

        internal static Type GetElementType(Type seqType)
        {
            Type ienum = FindIEnumerable(seqType);
            if (ienum == null) return seqType;
            return ienum.GetGenericArguments()[0];
        }

        private static Type FindIEnumerable(Type typeOfSeq)
        {
            if (typeOfSeq == null || typeOfSeq == typeof(string))
                return null;
            if (typeOfSeq.IsArray)
                return typeof(IEnumerable<>).MakeGenericType(typeOfSeq.GetElementType());
            if (typeOfSeq.IsGenericType)
            {
                foreach (Type arg in typeOfSeq.GetGenericArguments())
                {
                    Type ienum = typeof(IEnumerable<>).MakeGenericType(arg);
                    if (ienum.IsAssignableFrom(typeOfSeq))
                    {
                        return ienum;
                    }
                }
            }
            Type[] ifaces = typeOfSeq.GetInterfaces();
            if (ifaces.Length > 0)
            {
                foreach (Type iface in ifaces)
                {
                    Type ienum = FindIEnumerable(iface);
                    if (ienum != null) return ienum;
                }
            }
            if (typeOfSeq.BaseType != null && typeOfSeq.BaseType != typeof(object))
            {
                return FindIEnumerable(typeOfSeq.BaseType);
            }
            return null;
        }

        public static bool  CheckGenericParameterAttributes(Type param, Type targArg)
        {
            string retval;
            GenericParameterAttributes gpa = param.GenericParameterAttributes;
            GenericParameterAttributes variance = gpa &
                GenericParameterAttributes.VarianceMask;

//            // Select the variance flags. 
//            if (variance == GenericParameterAttributes.None)
//                retval = "No variance flag;";
//            else
//            {
//                if ((variance & GenericParameterAttributes.Covariant) != 0)
//                    retval = "Covariant;";
//                else
//                    retval = "Contravariant;";
//            }

            // Select 
            GenericParameterAttributes constraints = gpa &
                GenericParameterAttributes.SpecialConstraintMask;

            var ret = true;
            if (constraints == GenericParameterAttributes.None)
                return true;
            
            if ((constraints & GenericParameterAttributes.ReferenceTypeConstraint) != 0)
                ret = ret && targArg.IsClass;
            if ((constraints & GenericParameterAttributes.NotNullableValueTypeConstraint) != 0)
                ret = ret && targArg.IsValueType;
            if ((constraints & GenericParameterAttributes.DefaultConstructorConstraint) != 0)
                ret = ret && targArg.GetConstructor(Type.EmptyTypes) != null;

            return ret;
        }

    }
}
