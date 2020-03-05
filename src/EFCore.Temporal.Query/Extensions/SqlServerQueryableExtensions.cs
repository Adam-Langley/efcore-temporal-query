using Microsoft.EntityFrameworkCore.Query.Internal;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Microsoft.EntityFrameworkCore
{

    public static class SqlServerQueryableExtensions
    {
        public static readonly MethodInfo AsOfMethodInfo
          = typeof(SqlServerQueryableExtensions).GetTypeInfo().GetDeclaredMethod(nameof(AsOf));

        public static IQueryable<TEntity> AsOf<TEntity>(this IQueryable<TEntity> source, DateTime date) where TEntity : class
        {
            return
              source.Provider is EntityQueryProvider
                ? source.Provider.CreateQuery<TEntity>(
                  Expression.Call(
                    instance: null,
                    method: AsOfMethodInfo.MakeGenericMethod(typeof(TEntity)),
                    arg0: source.Expression,
                    arg1: Expression.Constant(date)))
                : source;
        }
    }
}
