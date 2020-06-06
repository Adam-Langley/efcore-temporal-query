using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using System;
using System.Reflection;

namespace EntityFrameworkCore.TemporalTables.Query
{
    public class AsOfSqlExpressionFactory : SqlExpressionFactory
    {
        public AsOfSqlExpressionFactory(SqlExpressionFactoryDependencies dependencies) : base(dependencies)
        {
        }

        public override SelectExpression Select(IEntityType entityType)
        {
            if (entityType.FindAnnotation(SqlServerAsOfEntityTypeBuilderExtensions.ANNOTATION_TEMPORAL) != null)
            {
                var asOfTableExpression = new AsOfTableExpression(
                    entityType.GetTableName(),
                    entityType.GetSchema(),
                    entityType.GetTableName().ToLower().Substring(0, 1));

                var selectContructor = typeof(SelectExpression).GetConstructor(BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance, null, new Type[] { typeof(IEntityType), typeof(TableExpressionBase) }, null);
                var select = (SelectExpression)selectContructor.Invoke(new object[] { entityType, asOfTableExpression });

                var privateInitializer = typeof(SqlExpressionFactory).GetMethod("AddConditions", BindingFlags.NonPublic | BindingFlags.Instance);
                privateInitializer.Invoke(this, new object[] { select, entityType, null, null });

                return select;
            }

            return base.Select(entityType);
        }

    }
}
