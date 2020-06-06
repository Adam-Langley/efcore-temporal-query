using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;

namespace EntityFrameworkCore.TemporalTables.Query
{
    public class AsOfQuerySqlGeneratorFactory : IQuerySqlGeneratorFactory
    {
        private readonly QuerySqlGeneratorDependencies _dependencies;

        public AsOfQuerySqlGeneratorFactory(
            [NotNull] QuerySqlGeneratorDependencies dependencies)
        {
            _dependencies = dependencies;
        }

        public QuerySqlGenerator Create()
        {
            return new AsOfQuerySqlGenerator(_dependencies, null, null);
        }
    }

}
