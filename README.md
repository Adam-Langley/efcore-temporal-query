# efcore-temporal-query

Linq extensions to Entity Framework Core 3.1 to support [Microsoft SQL Server Temporal Table](https://docs.microsoft.com/en-us/sql/relational-databases/tables/temporal-tables) querying.

This extension was created to supplement the work planned by Microsoft, discussed [here](https://github.com/dotnet/efcore/issues/4693).

*Note: This library does not facilitate schema alteration (such as through EF migrations).
That capability can be supplemented through other libraries, such as [EntityFrameworkCore.TemporalTables](https://github.com/findulov/EntityFrameworkCore.TemporalTables).*

# Installation
[![NuGet](https://img.shields.io/nuget/v/Dabble.EntityFrameworkCore.Temporal.Query.svg)](https://www.nuget.org/packages/Dabble.EntityFrameworkCore.Temporal.Query/)
[![NuGet](https://img.shields.io/nuget/dt/Dabble.EntityFrameworkCore.Temporal.Query.svg)](https://www.nuget.org/packages/Dabble.EntityFrameworkCore.Temporal.Query/)

# Getting Started...
The new extension methods you will be using have been placed in the `Microsoft.EntityFrameworkCore` namespace
to assist with discoverability.

Following are the 3 mandatory steps to querying Microft SQL Server Temporal Tables.


## 1. Entity Configuration

Use the `x.HasTemporalTable()` extension method to mark your desired entities as candidates for the `FOR SYSTEM TIME` (a.k.a temporal) syntax.
This step alone will not cause the temporal SQL to be generated.
The reason being - when applying a temporal time to a linq query, it will cause the temporal syntax to be applied to all compatible tables in that query.
This 'opt in' mechanism allows you to ensure that the query compiler does _not_ apply the syntax to tables that do not support it.

```csharp
using Microsoft.EntityFrameworkCore;

...

modelBuilder.Entity<Customer>(b => {
    b.HasTemporalTable();
});

modelBuilder.Entity<Address>(b => {
    b.HasTemporalTable();
});
```


## 2. DbContext Initialization

Use the `x.EnableTemporalTableQueries()` extension to replace the necessary EF pipeline services responsible for generating the SQL syntax at runtime.

*IMPORTANT* - `efcore-temporal-query` does not support EF internal service providers (user supplied ones). If you are following these instructions, and temporal SQL is not being generated, please check you are not calling `DbContextOptionsBuilder.UseInternalServiceProvider` somewhere in your code.

```csharp
using Microsoft.EntityFrameworkCore;

...

public static void Configure(DbContextOptionsBuilder<DbContext> builder, string connectionString)
{
    builder
        .UseSqlServer(connectionString)
        .EnableTemporalTableQueries(); // here we enable the temporal table sql generator
}
```

## 3. Querying

Use the `IQueryable<T>.AsOf(DateTime)` extension to specialize a linq expression to a particular point in time.

The following is an example of querying a customer record from a Temporal Table at a specific
time, including an Address record from that same time.

The temporal state is applied to the entire query, meaning the extension method need only be called once (anywhere in the fluent statement).

All joined relationships will have the `FOR SYSTEM_TIME` predicate applied to the generated SQL.

```csharp
using Microsoft.EntityFrameworkCore;

...

var yesterdaysCustomerRecord = _db.Customers
                                    .Include(x => x.Address)
                                    .AsOf(DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)));
                                    
// or place the 'AsOf' first if you prefer
var yesterdaysCustomerRecord = _db.Customers
                                    .AsOf(DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)))
                                    .Include(x => x.Address);

```

Resulting SQL (pseudo-example for demonstrative purposes)

```sql
SELECT * FROM Customer FOR SYSTEM TIME AS OF @p0 c 
LEFT JOIN 
Address FOR SYSTEM TIME AS OF @p0 a 
ON c.Id = a.CustomerId
```

# Roadmap Features
1. Runtime per-join configuration, e.g.
```csharp
var yesterdaysCustomerRecordWithTodaysAddress = _db.Customers
                                    .AsOf(DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)))
                                    .IncludeAsOfNow(x => x.Address);
                                    

var yesterdaysCustomerRecordWithAnOlderAddress = _db.Customers
                                    .AsOf(DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)))
                                    .IncludeAsOf(x => x.Address, DateTime.UtcNow.Subtract(TimeSpan.FromHours(1)));
                                    
```
