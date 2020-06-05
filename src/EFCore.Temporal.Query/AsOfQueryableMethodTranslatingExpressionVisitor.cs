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
        private ParameterExpression _asOfDateParameter;

        public AsOfQueryableMethodTranslatingExpressionVisitor(
            QueryableMethodTranslatingExpressionVisitorDependencies dependencies,
            RelationalQueryableMethodTranslatingExpressionVisitorDependencies relationalDependencies,
            IModel model,
            ParameterExpression asOfDateParameter = null
            ) : base(dependencies, relationalDependencies, model)
        {
            _dependencies = dependencies;
            _relationalDependencies = relationalDependencies;
            _model = model;
            _asOfDateParameter = asOfDateParameter;

            var sqlExpressionFactory = relationalDependencies.SqlExpressionFactory;
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        protected override Expression VisitConstant(ConstantExpression constantExpression)
        {
            var result = base.VisitConstant(constantExpression);
            if (null != _asOfDateParameter && result is ShapedQueryExpression shapedExpression)
            {
                // attempt to apply the captured date parameter to any select-from-table expressions
                shapedExpression.TrySetDateParameter(_asOfDateParameter);
            }
            return result;
        }

        protected override QueryableMethodTranslatingExpressionVisitor CreateSubqueryVisitor()
        {
            return new AsOfQueryableMethodTranslatingExpressionVisitor(
                _dependencies,
                _relationalDependencies,
                _model,
                _asOfDateParameter);
        }

        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            var methodInfo = methodCallExpression.Method;

            if (methodInfo.DeclaringType == typeof(SqlServerQueryableExtensions))
            {
                switch (methodInfo.Name)
                {
                    case nameof(SqlServerQueryableExtensions.AsOf):
                        // capture the date parameter for use by all AsOfTableExpression instances
                        _asOfDateParameter = Visit(methodCallExpression.Arguments[1]) as ParameterExpression;
                        return Visit(methodCallExpression.Arguments[0]);
                }
            }

            return base.VisitMethodCall(methodCallExpression);
        }
    }
}
