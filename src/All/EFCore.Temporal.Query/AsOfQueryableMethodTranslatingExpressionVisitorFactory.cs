using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;

namespace EntityFrameworkCore.TemporalTables.Query
{
    public class AsOfQueryableMethodTranslatingExpressionVisitorFactory : IQueryableMethodTranslatingExpressionVisitorFactory
    {
        private readonly QueryableMethodTranslatingExpressionVisitorDependencies _dependencies;
        private readonly RelationalQueryableMethodTranslatingExpressionVisitorDependencies _relationalDependencies;

        public AsOfQueryableMethodTranslatingExpressionVisitorFactory(
            QueryableMethodTranslatingExpressionVisitorDependencies dependencies,
            RelationalQueryableMethodTranslatingExpressionVisitorDependencies relationalDependencies)
        {
            _dependencies = dependencies;
            _relationalDependencies = relationalDependencies;
        }
        public QueryableMethodTranslatingExpressionVisitor Create(IModel model)
        {
            return new AsOfQueryableMethodTranslatingExpressionVisitor(_dependencies, _relationalDependencies, model);
        }
    }
}
