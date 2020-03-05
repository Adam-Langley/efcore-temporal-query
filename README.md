# efcore-temporal-query

Linq extensions to Entity Framework Core 3.1 to support Microsoft SQL Server Temporal Table Querying

Note: This library does not facilitate the creation of temporal table schemas (such as through Ef migrations).
It's suggested that you use another library, such as [EntityFrameworkCore.TemporalTables](https://github.com/findulov/EntityFrameworkCore.TemporalTables).

# Installation
[![NuGet](https://img.shields.io/nuget/v/Dabble.EntityFrameworkCore.Temporal.Query.svg)](https://www.nuget.org/packages/Dabble.EntityFrameworkCore.Temporal.Query/)

# Getting Started...
The extension methods you will be using have been placed in the 'Microsoft.EntityFrameworkCore' namespace
to assist in dicoverability.

Following are the 3 manditory steps to querying Microft SQL Server Temporal Tables.


## 1. Entity Configuration

This 'marks' these entities as candidates for the "FOR SYSTEM TIME" syntax - this ensures
that the query compiler does not apply this syntax to tables that do not support it.

```
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

This replaces the necessary EF pipeline services to produce the new SQL syntax.

```
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

The following is an example of querying a customer record from a Temporal Table at a specific
time, including an Address record from that same time.
All joined relationships will have the "FOR SYSTEM_TIME" predicate applied to the generated SQL.

```
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

```
SELECT * FROM Customer FOR SYSTEM TIME AS OF '2020-02-28T11:00:00' c 
LEFT JOIN 
Address FOR SYSTEM TIME AS OF '2020-02-28T11:00:00' a 
ON c.Id = a.CustomerId
```

## 4. Roadmap Features
1. Runtime per-join configuration, e.g.
```
var yesterdaysCustomerRecordWithTodaysAddress = _db.Customers
                                    .AsOf(DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)))
                                    .IncludeAsOfNow(x => x.Address);
                                    

var yesterdaysCustomerRecordWithAnOlderAddress = _db.Customers
                                    .AsOf(DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)))
                                    .IncludeAsOf(x => x.Address, DateTime.UtcNow.Subtract(TimeSpan.FromHours(1)));
                                    
```
