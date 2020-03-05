using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace EntityFrameworkCore.TemporalTables.Query
{
    class MyRelationalCommandBuilderFactory : IRelationalCommandBuilderFactory
    {
        private readonly IRelationalCommandBuilder _builder;

        public MyRelationalCommandBuilderFactory(IRelationalCommandBuilder builder)    
        {
            _builder = builder;
        }

        public IRelationalCommandBuilder Create()
        {
            return _builder;
        }
    }
    public class AsOfQuerySqlGenerator : SqlServerQuerySqlGenerator
    {
        private ISqlGenerationHelper _sqlGenerationHelper;
        private readonly RelationalQueryContext _ctx;
        private IRelationalCommandBuilder _commandbuilder;

        public AsOfQuerySqlGenerator(
            QuerySqlGeneratorDependencies dependencies, 
            QuerySqlGenerator inner, 
            RelationalQueryContext ctx)
            : base(new QuerySqlGeneratorDependencies(
                new MyRelationalCommandBuilderFactory(dependencies.RelationalCommandBuilderFactory.Create()), dependencies.SqlGenerationHelper))
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
            Sql.Append(_sqlGenerationHelper.DelimitIdentifier(tableExpression.Name, tableExpression.Schema));
            if (tableExpression.DateParameter != null)
            {
                var name = "__ef_temporal" + tableExpression.DateParameter.Name;
                Sql.Append($" FOR SYSTEM_TIME AS OF @{name}"); //2020-02-28T11:00:00

                if (!_commandbuilder.Parameters.Any(x => x.InvariantName == tableExpression.DateParameter.Name))
                    _commandbuilder.AddParameter(tableExpression.DateParameter.Name, name);
            }
            Sql
                .Append(AliasSeparator)
                .Append(_sqlGenerationHelper.DelimitIdentifier(tableExpression.Alias));

            return tableExpression;
        }
    }

}
