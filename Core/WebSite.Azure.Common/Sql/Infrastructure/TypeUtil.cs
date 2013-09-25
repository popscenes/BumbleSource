using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Website.Azure.Common.Sql.Infrastructure
{
    public static class TypeUtil
    {
        public static string GetAssemblyQualifiedNameWithoutVer(this Type type)
        {
            var name = type.AssemblyQualifiedName;
            var verIndex = name.IndexOf(", Version=", System.StringComparison.Ordinal);
            if (verIndex <= 0)
                return name;

            return name.Substring(0, verIndex);
        }

        public static Delegate GetConstructorFor(this Type type)
        {
            var ctor = type.GetConstructor(new Type[0]);
            if (ctor == null) return null;
            var funcType = typeof(Func<>).MakeGenericType(type);
            var lamda = Expression.Lambda(funcType, Expression.New(ctor));
            return lamda.Compile();
        }

        public static Delegate CreateObjectInitDelegate(this Type type, params string[] properties)
        {
            var newType = Expression.New(type);

            var bindings = new List<MemberBinding>();
            var parameters = new List<ParameterExpression>();
            foreach (var property in properties)
            {
                var memberInfo =
                    type.GetMember(property)[0] as PropertyInfo;
                if (memberInfo == null) throw new ArgumentException("Invalid Property " + property, "properties");

                var instanceParam = Expression.Parameter(memberInfo.PropertyType, property);
//                var parameter = Expression.Parameter(typeof(object), property);
//                var instanceParam = Expression.Convert(parameter, memberInfo.PropertyType);

                var memberBinding = Expression.Bind(
                    memberInfo,
                    instanceParam);
                bindings.Add(memberBinding);
                parameters.Add(instanceParam);
//                parameters.Add(parameter);

            }

            var memberInitExpression = Expression.MemberInit(newType, bindings);

            var paramTypes = parameters.Select(param => param.Type).ToList();
            //var paramTypes = bindings.Select(binding => typeof(object)).ToList();
            paramTypes.Add(type);

            var funcType = GetFuncForArgCount(bindings.Count).MakeGenericType(paramTypes.ToArray());
            var lamda = Expression.Lambda(funcType, memberInitExpression, parameters.ToArray());

//            Trace.WriteLine(lamda.ToString());

            return lamda.Compile();
        }

        internal static Type GetFuncForArgCount(int argCount)
        {
            switch (argCount)
            {
                case 0:
                    return typeof (Func<>);
                case 1:
                    return typeof(Func<,>);
                case 2:
                    return typeof(Func<,,>);
                case 3:
                    return typeof(Func<,,,>);
                case 4:
                    return typeof(Func<,,,,>);
                case 5:
                    return typeof(Func<,,,,,>);
                case 6:
                    return typeof(Func<,,,,,,>);
                default:
                    throw new ArgumentException("Add more cases if you need more intializers", "argCount");
            }
        }

        public static Func<object, object> GetPropertyGetterFn(this PropertyInfo propertyInfo)
        {
            var getMethodInfo = propertyInfo.GetGetMethod(true);
            if (getMethodInfo == null) return null;
        
            var oInstanceParam = Expression.Parameter(typeof(object), "oInstanceParam");
            var instanceParam = Expression.Convert(oInstanceParam, propertyInfo.DeclaringType);

            var exprCallPropertyGetFn = Expression.Call(instanceParam, getMethodInfo);
            var oExprCallPropertyGetFn = Expression.Convert(exprCallPropertyGetFn, typeof(object));

            var propertyGetFn = Expression.Lambda<Func<object, object>>
                (
                    oExprCallPropertyGetFn,
                    oInstanceParam
                ).Compile();

            return propertyGetFn;
        }

        public static PropertyInfo GetPropertyInfoFor<TSource, TRet>(this Expression<Func<TSource, TRet>> forMember)
        {
//            var lamb = forMember as LambdaExpression;
//            if(lamb.Body.NodeType == ExpressionType.MemberAccess)
//            {
//                var ret = lamb.Body as MemberExpression;
//                return ret.Member as PropertyInfo;
//            }
//
//            if (lamb.Body.NodeType == ExpressionType.Parameter)
//            {
//                var ret = lamb.Body as ParameterExpression;
//            }

            var visit = new FindProperty();
            visit.Visit(forMember);

            return visit.RootProperty;
        }

        internal class FindProperty : ExpressionVisitor
        {
            public PropertyInfo RootProperty { get; set; }
            protected override Expression VisitMember(MemberExpression node)
            {
                var ret = base.VisitMember(node);
                var prop = node.Member as PropertyInfo;
                RootProperty = prop ?? RootProperty;
                return ret;
            }
        }

        public static bool IsEnumerable(this PropertyInfo propertyInfo)
        {
            return typeof (IEnumerable).IsAssignableFrom(propertyInfo.PropertyType);
        }

        public static Type GetUnderlyingTypeIfNullable(this Type type)
        {
            return Nullable.GetUnderlyingType(type) ?? type;
        }

        public static Type GetUnderlyingType(this Type enumType)
        {
            enumType = enumType.GetUnderlyingTypeIfNullable();
            return (enumType.GetUnderlyingTypeEnumerable() ?? enumType).GetUnderlyingTypeIfNullable();
        }

        public static Type GetUnderlyingTypeEnumerable(this Type enumType)
        {
            if (enumType == null)
                throw new ArgumentNullException("enumType");
            var type = (Type)null;
            var enumer =
                enumType.GetInterfaces().FirstOrDefault(
                    t =>
                    t.IsGenericType &&
                    object.ReferenceEquals((object) t.GetGenericTypeDefinition(), (object) typeof (IEnumerable<>)));

            if(enumer != null)
                type = enumer.GetGenericArguments()[0];

            return type;
        }

        public static bool IsScalar<T>()
        {
            return typeof(T).IsScalar();
        }

        public static bool IsScalar(this Type enumType)
        {
            return enumType.IsValueType || enumType == typeof(string);
        }
    }
}