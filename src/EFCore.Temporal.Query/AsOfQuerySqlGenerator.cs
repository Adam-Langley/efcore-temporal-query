using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;

namespace EntityFrameworkCore.TemporalTables.Query
{
    [SuppressMessage("Usage", "EF1001:Internal EF Core API usage.")]
    public class AsOfQuerySqlGenerator : SqlServerQuerySqlGenerator
    {
        const string TEMPORAL_PARAMETER_PREFIX = "__ef_temporal";

        private ISqlGenerationHelper _sqlGenerationHelper;
        private readonly RelationalQueryContext _ctx;
        private IRelationalCommandBuilder _commandbuilder;

        [SuppressMessage("Usage", "EF1001:Internal EF Core API usage.")]
        public AsOfQuerySqlGenerator(
            QuerySqlGeneratorDependencies dependencies, 
            QuerySqlGenerator inner, 
            RelationalQueryContext ctx)
            : base(new QuerySqlGeneratorDependencies(
                new SingletonRelationalCommandBuilderFactory(dependencies.RelationalCommandBuilderFactory), dependencies.SqlGenerationHelper))
        {
            _sqlGenerationHelper = dependencies.SqlGenerationHelper;
            _ctx = ctx;
            _commandbuilder = this.Dependencies.RelationalCommandBuilderFactory.Create();
        }

        protected override Expression VisitExtension(Expression extensionExpression)
        {
            switch (extensionExpression)
            {
                case AsOfTableExpression tableExpression:
                    return VisitAsOfTable(tableExpression);
            }

            return base.VisitExtension(extensionExpression);
        }

        protected virtual Expression VisitAsOfTable(AsOfTableExpression tableExpression)
        {
            // This method was modeled on "SqlServerQuerySqlGenerator.VisitTable".
            // Where we deviate, is after printing the table name, we check if temporal constraints
            // need to be applied.

            Sql.Append(_sqlGenerationHelper.DelimitIdentifier(tableExpression.Name, tableExpression.Schema));

            if (tableExpression.AsOfDate != null)
            {
                var name = TEMPORAL_PARAMETER_PREFIX + tableExpression.AsOfDate.Name;
                Sql.Append($" FOR SYSTEM_TIME AS OF @{name}"); //2020-02-28T11:00:00

                if (!_commandbuilder.Parameters.Any(x => x.InvariantName == tableExpression.AsOfDate.Name))
                    _commandbuilder.AddParameter(tableExpression.AsOfDate.Name, name);
            }
            
            Sql
                .Append(AliasSeparator)
                .Append(_sqlGenerationHelper.DelimitIdentifier(tableExpression.Alias));

            return tableExpression;
        }
    }

}
