using System;
using System.Linq.Expressions;

namespace WebSite.Application.Extension.Expression
{
    public static class ExpressionExtension
    {
        public static Expression<Func<T, bool>> AndAlso<T>(
            this Expression<Func<T, bool>> expr1,
            Expression<Func<T, bool>> expr2)
        {
            if (expr2 == null)
                return expr1;
            // need to detect whether they use the same
            // parameter instance; if not, they need fixing
            ParameterExpression param = expr1.Parameters[0];
            if (ReferenceEquals(param, expr2.Parameters[0]))
            {
                // simple version
                return System.Linq.Expressions.Expression.Lambda<Func<T, bool>>(
                    System.Linq.Expressions.Expression.AndAlso(expr1.Body, expr2.Body), param);
            }
            // otherwise, keep expr1 "as is" and invoke expr2
            return System.Linq.Expressions.Expression.Lambda<Func<T, bool>>(
                System.Linq.Expressions.Expression.AndAlso(
                    expr1.Body,
                    System.Linq.Expressions.Expression.Invoke(expr2, param)), param);
        }

        public static Expression<Func<T, bool>> AndAlsoSameParam<T>(
            this Expression<Func<T, bool>> expr1,
            Expression<Func<T, bool>> expr2)
        {
            if (expr2 == null)
                return expr1;

            ParameterExpression param = expr1.Parameters[0];
            return System.Linq.Expressions.Expression.Lambda<Func<T, bool>>(
                System.Linq.Expressions.Expression.AndAlso(expr1.Body, expr2.Body), param);
        }
    }
}