using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.FSharp.Collections;
using Website.FunctionalLib;
using Website.Infrastructure.Util.Extension;

namespace Website.Infrastructure.Types
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

        public static List<Type> GetAllSubTypesFrom(Type queryTyp, params Assembly[] typeAssemblies)
        {
            return typeAssemblies.SelectMany(assembly => assembly.DefinedTypes)
            .Select(info => info.AsType())
            .Where(queryTyp.IsAssignableFrom)
            .ToList();
        }

        public static List<Type> GetExpandedTypesUsing(Type queryTyp, params Assembly[] typeAssemblies)
        {
            var argTypes = typeAssemblies.SelectMany(assembly => assembly.DefinedTypes)
                        .Select(info => info.AsType()).ToList();

            var ret = new List<Type>();
            TypeUtil.Expand(queryTyp, argTypes, ret);
            return ret;
        }

        public static List<Type> Expand(Type queryTyp, List<Type> argTypes)
        {
            var ret = new List<Type>();
            TypeUtil.Expand(queryTyp, argTypes, ret);
            return ret;
        }

        public static List<Type> GetExpandedImplementorsUsing(Type queryTyp, params Assembly[] typeAssemblies)
        {
            var argTypes = typeAssemblies.SelectMany(assembly => assembly.DefinedTypes)
                        .Select(info => info.AsType()).ToList();

            var subType = argTypes.Where(a => a
                                             .GetInterfaces()
                                             .Any(arg
                                                  => queryTyp.IsAssignableFrom(arg)
                                                     || (arg.IsGenericType && queryTyp.IsGenericTypeDefinition && queryTyp == arg.GetGenericTypeDefinition())
                                             ));

            return TypeUtil.ExpandGenericTypes(
                subType.ToList(), argTypes.Where(arg => !subType.Contains(arg)).ToList());
        }

        public static List<Type> ExpandGenericTypes(List<Type> argTypes)
        {
            var ret = new List<Type>(argTypes.Where(a => !a.IsGenericTypeDefinition));

            argTypes.ForEach(obj => Expand(obj, ret, ret));

            return ret;
        }

        public static List<Type> ExpandGenericTypes(List<Type> genTypes, List<Type> argTypes)
        {
            var ret = new List<Type>();

            genTypes.ForEach(obj => Expand(obj, argTypes, ret));

            return ret;
        }

        public static void Expand(Type type, IEnumerable<Type> argTypes, ICollection<Type> retTypes)
        {
            if (type.IsInterface) return;
            if (!type.IsGenericTypeDefinition)
            {
                retTypes.Add(type);
                return;
            }

            var args = type.GetGenericArguments();
            var compatArgs = args.Select(a =>
            {
                var c = a.GetGenericParameterConstraints();
                return argTypes.Where(t => !t.IsGenericTypeDefinition && TypeUtil.CheckGenericParameterAttributes(a, t) 
                                            && 
                                           c.All(
                                               constype =>
                                               constype.IsAssignableFrom(t) /*||
                                               constype.IsGenericType && MatchGenericConstraint(constype, type) */             
                                               )).ToFSharpList();
            }).ToFSharpList();

//
//                                                           constype.IsAssignableFrom(t) ||
//                                               constype.IsGenericType && t.GetInterfaces().Any(i =>
//                                                   i.IsGenericType && !i.IsGenericTypeDefinition &&
//                                                   i.GetGenericTypeDefinition() == constype.GetGenericTypeDefinition())
//                                               )).ToFSharpList();

            var func = new Lists();
            var all = func.Cartesian<FSharpList<Type>, Type>(compatArgs);
            if (all.Count() > 500)
                throw new ArgumentException("Limit Types");

            foreach (var argset in all)
            {
                try
                {
                    var add = type.MakeGenericType(argset.ToArray());
                    if (!retTypes.Contains(add))
                        retTypes.Add(add);
                }catch(Exception){}
            }
        }

        private static bool MatchGenericConstraint(Type constraint, Type type)
        {
            var args = constraint.GetGenericArguments();
            return args.All(a =>
                {
                    var c = a.GetGenericParameterConstraints();
                    return
                        type.GetGenericArguments()
                        .Any(t => 
                            !t.IsGenericTypeDefinition 
                            && TypeUtil.CheckGenericParameterAttributes(a, t)
                            && c.All(constype =>
                                constype.IsAssignableFrom(t) ||
                                constype.IsGenericType && MatchGenericConstraint(constype, t)));
                });
        }

        public static bool IsFromNameSpaceContaining(this Type type, string name)
        {
            return !string.IsNullOrWhiteSpace(type.Namespace) && type.Namespace.ToLower().Contains(name.ToLower());
        }

        public static bool NameLike(this Type type, string name)
        {
            return !string.IsNullOrWhiteSpace(type.Name) && type.Name.ToLower().Contains(name.ToLower());
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
