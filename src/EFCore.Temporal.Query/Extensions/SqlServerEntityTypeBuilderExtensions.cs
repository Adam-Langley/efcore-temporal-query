using EntityFrameworkCore.TemporalTables.Query;
using Microsoft.EntityFrameworkCore.Infrastructure;
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

        public static DbContextOptionsBuilder EnableTemporalTableQueries<TContext>([NotNull] this DbContextOptionsBuilder optionsBuilder)
        {
            // If service provision is NOT being performed internally, we cannot replace services.
            var coreOptions = optionsBuilder.Options.GetExtension<CoreOptionsExtension>();
            if (coreOptions.InternalServiceProvider == null)
            {
                return optionsBuilder
                    // replace the service responsible for generating SQL strings
                    .ReplaceService<IQuerySqlGeneratorFactory, AsOfQuerySqlGeneratorFactory>()
                    // replace the service responsible for traversing the Linq AST (a.k.a Query Methods)
                    .ReplaceService<IQueryableMethodTranslatingExpressionVisitorFactory, AsOfQueryableMethodTranslatingExpressionVisitorFactory>()
                    // replace the service responsible for providing instances of SqlExpressions
                    .ReplaceService<ISqlExpressionFactory, AsOfSqlExpressionFactory>();
            }
            else 
                return optionsBuilder;
        }
    }
}
