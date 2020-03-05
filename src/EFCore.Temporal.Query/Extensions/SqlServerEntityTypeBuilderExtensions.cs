using EntityFrameworkCore.TemporalTables.Query;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Query;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.EntityFrameworkCore
{
    public static class SqlServerEntityTypeBuilderExtensions
    {
        public static string ANNOTATION_TEMPORAL = "IS_TEMPORAL_TABLE";

        public static EntityTypeBuilder<TEntity> HasTemporalTable<TEntity>(this EntityTypeBuilder<TEntity> entity) where TEntity : class
        {
            entity.Metadata.SetAnnotation(ANNOTATION_TEMPORAL, true);
            return entity;
        }

        public static DbContextOptionsBuilder<TContext> EnableTemporalTableQueries<TContext>([NotNull] this DbContextOptionsBuilder<TContext> optionsBuilder) where TContext : DbContext
        {
            return optionsBuilder
                .ReplaceService<IQuerySqlGeneratorFactory, AsOfQuerySqlGeneratorFactory>()
                .ReplaceService<IQueryableMethodTranslatingExpressionVisitorFactory, AsOfQueryableMethodTranslatingExpressionVisitorFactory>()
                .ReplaceService<ISqlExpressionFactory, AsOfSqlExpressionFactory>();
        }
    }
}
