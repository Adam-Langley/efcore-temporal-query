using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using System;
using System.Linq.Expressions;

namespace EntityFrameworkCore.TemporalTables.Query
{
    /// <summary>
    /// A derived implementation of TableExpressionBase which allows us to attach some meta-data to
    /// the TableExpressionBase - namely the "AsOfDate" property.
    /// </summary>
    public class AsOfTableExpression : TableExpressionBase
    {
        public string Name { get; }
        public string Schema { get; }
        /// <summary>
        /// Gets and sets the parameter used to constrain a query to a specific temporal period.
        /// </summary>
        public ParameterExpression AsOfDate { get; set; }

        public AsOfTableExpression(string name, string schema, string alias) : base(alias)
        {
            Name = name;
            Schema = schema;
        }

        public override void Print(ExpressionPrinter expressionPrinter)
        {
            if (!string.IsNullOrEmpty(Schema))
            {
                expressionPrinter.Append(Schema).Append(".");
            }

            expressionPrinter.Append(Name).Append(" AS ").Append(Alias);
        }

        public override bool Equals(object obj)
             // This should be reference equal only.
             => obj != null && ReferenceEquals(this, obj);

        public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), Name, Schema);
    }
}
