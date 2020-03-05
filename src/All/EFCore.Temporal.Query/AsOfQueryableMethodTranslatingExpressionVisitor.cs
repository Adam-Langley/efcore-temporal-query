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
            var method = methodCallExpression.Method;
            if (method.DeclaringType == typeof(SqlServerQueryableExtensions))
            {
                switch (method.Name)
                {
                    case nameof(SqlServerQueryableExtensions.AsOf):
                        // create an expression....
                        // store expression path
                        var source = Visit(methodCallExpression.Arguments[0]);
                        var dateParameter = Visit(methodCallExpression.Arguments[1]) as ParameterExpression;
                        if (source is ShapedQueryExpression shaped)
                        {
                            if (shaped.QueryExpression is SelectExpression select)
                            {
                                foreach (AsOfTableExpression asOfTable in select.Tables.OfType<AsOfTableExpression>())
                                {
                                    asOfTable.DateParameter = dateParameter;
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
