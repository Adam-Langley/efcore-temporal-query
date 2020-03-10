using Microsoft.EntityFrameworkCore.Storage;
using System;

namespace EntityFrameworkCore.TemporalTables.Query
{
    public class SingletonRelationalCommandBuilderFactory : IRelationalCommandBuilderFactory
    {
        private Lazy<IRelationalCommandBuilder> _builder;

        public SingletonRelationalCommandBuilderFactory(IRelationalCommandBuilderFactory innerFactory)
        {
            _builder = new Lazy<IRelationalCommandBuilder>(() => innerFactory.Create());
        }

        public IRelationalCommandBuilder Create()
        {
            return _builder.Value;
        }
    }

}
