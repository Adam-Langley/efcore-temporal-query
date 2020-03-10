using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Temporal.Query.Extensions.Internal;

namespace EntityFrameworkCore.TemporalTables.Query
{
    /// <summary>
    /// This class is responsible for traversing the Linq query extension methods, searching for any
    /// calls to the "AsOf" extension.
    /// When it encounters them, it will process the expressions they are attached to, and find any "AsOfTableExpression" instances,
    /// then set the "AsOfDate" property of those tables.
    /// </summary>
    public class AsOfQueryableMethodTranslatingExpressionVisitor : RelationalQueryableMethodTranslatingExpressionVisitor
    {
        private readonly QueryableMethodTranslatingExpressionVisitorDependencies _dependencies;
        private readonly RelationalQueryableMethodTranslatingExpressionVisitorDependencies _relationalDependencies;
        private readonly IModel _model;
        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        public AsOfQueryableMethodTranslatingExpressionVisitor(
            QueryableMethodTranslatingExpressionVisitorDependencies dependencies,
            RelationalQueryableMethodTranslatingExpressionVisitorDependencies relationalDependencies,
            IModel model
            ) : base(dependencies, relationalDependencies, model)
        {
            _dependencies = dependencies;
            _relationalDependencies = relationalDependencies;
            _model = model;

            var sqlExpressionFactory = relationalDependencies.SqlExpressionFactory;
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        protected override ShapedQueryExpression TranslateLeftJoin(ShapedQueryExpression outer, ShapedQueryExpression inner, LambdaExpression outerKeySelector, LambdaExpression innerKeySelector, LambdaExpression resultSelector)
        {
            ParameterExpression asOfParameter = null;

            if (outer.TryGetDateParameter(out asOfParameter))
            {
                inner.TrySetDateParameter(asOfParameter);
            }

            return base.TranslateLeftJoin(outer, inner, outerKeySelector, innerKeySelector, resultSelector);
        }

        protected override ShapedQueryExpression TranslateGroupJoin(ShapedQueryExpression outer, ShapedQueryExpression inner, LambdaExpression outerKeySelector, LambdaExpression innerKeySelector, LambdaExpression resultSelector)
        {
            ParameterExpression asOfParameter = null;

            if (outer.TryGetDateParameter(out asOfParameter))
            {
                inner.TrySetDateParameter(asOfParameter);
            }           

            return base.TranslateGroupJoin(outer, inner, outerKeySelector, innerKeySelector, resultSelector);
        }

        protected override ShapedQueryExpression TranslateJoin(ShapedQueryExpression outer, ShapedQueryExpression inner, LambdaExpression outerKeySelector, LambdaExpression innerKeySelector, LambdaExpression resultSelector)
        {
            ParameterExpression asOfParameter = null;

            if (inner.TryGetDateParameter(out asOfParameter))
            {
                outer.TrySetDateParameter(asOfParameter);
            }

            return base.TranslateJoin(outer, inner, outerKeySelector, innerKeySelector, resultSelector);
        }

        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            var methodInfo = methodCallExpression.Method;

            if (methodInfo.DeclaringType == typeof(SqlServerQueryableExtensions))
            {
                switch (methodInfo.Name)
                {
                    case nameof(SqlServerQueryableExtensions.AsOf):
                        // create an expression....
                        // store expression path
                        var source = Visit(methodCallExpression.Arguments[0]);
                        if (source is ShapedQueryExpression shaped)
                        {
                            if (shaped.QueryExpression is SelectExpression select)
                            {
                                var dateParameter = Visit(methodCallExpression.Arguments[1]) as ParameterExpression;
                                foreach (AsOfTableExpression asOfTable in select.Tables.OfType<AsOfTableExpression>())
                                {
                                    asOfTable.AsOfDate = dateParameter;
                                }
                            }
                        }
                        return source;
                }
            }

            return base.VisitMethodCall(methodCallExpression);
        }
    }
}
