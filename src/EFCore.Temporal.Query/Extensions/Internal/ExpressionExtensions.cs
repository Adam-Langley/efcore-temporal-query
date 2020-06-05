using EntityFrameworkCore.TemporalTables.Query;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Microsoft.EntityFrameworkCore.Temporal.Query.Extensions.Internal
{
    public static class ExpressionExtensions
    {
        public static bool TrySetDateParameter(this ShapedQueryExpression shapedQuery, ParameterExpression dateParameter)
        {
            if (shapedQuery.QueryExpression is SelectExpression select)
            {
                foreach (var table in select.Tables.OfType<AsOfTableExpression>())
                {
                    if (null == table.AsOfDate)
                    {
                        table.AsOfDate = dateParameter;
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
