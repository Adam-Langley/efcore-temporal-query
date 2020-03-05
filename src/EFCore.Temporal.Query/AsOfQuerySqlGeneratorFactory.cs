using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;
using System.Diagnostics.CodeAnalysis;

namespace EntityFrameworkCore.TemporalTables.Query
{
    public class AsOfQuerySqlGeneratorFactory : IQuerySqlGeneratorFactory
    {
        private readonly IQueryableMethodTranslatingExpressionVisitorFactory _ss;
        private readonly QuerySqlGeneratorDependencies _dependencies;
        private readonly ISqlServerOptions _sqlServerOptions;
        private readonly QueryCompilationContext _queryCompilationContext;

        public AsOfQuerySqlGeneratorFactory(
            [NotNull] QuerySqlGeneratorDependencies dependencies,
            [NotNull] ISqlServerOptions sqlServerOptions)
        {
            _dependencies = dependencies;
            _sqlServerOptions = sqlServerOptions;
        }

        public QuerySqlGenerator Create()
        {
            return new AsOfQuerySqlGenerator(_dependencies, null, null);
        }
    }

}
