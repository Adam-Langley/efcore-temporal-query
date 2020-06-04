using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;

namespace EntityFrameworkCore.TemporalTables.Query
{
    public class AsOfQuerySqlGeneratorFactory : IQuerySqlGeneratorFactory
    {
        private readonly IQueryableMethodTranslatingExpressionVisitorFactory _ss;
        private readonly QuerySqlGeneratorDependencies _dependencies;
        private readonly ISqlServerOptions _sqlServerOptions;
        private readonly QueryCompilationContext _queryCompilationContext;

        public AsOfQuerySqlGeneratorFactory(
            QuerySqlGeneratorDependencies dependencies,
            ISqlServerOptions sqlServerOptions)
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
