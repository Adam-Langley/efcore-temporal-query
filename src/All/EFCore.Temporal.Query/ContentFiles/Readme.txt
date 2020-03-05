Getting Started...
==================

Following are the 3 manditory steps to querying Microft SQL Server Temporal Tables.


1. Entity Configuration
=======================
This 'marks' these entities as candidates for the "FOR SYSTEM TIME" syntax - this ensures
that the query compiler does not apply this syntax to tables that do not support it.

modelBuilder.Entity<Customer>(b => {
    b.HasTemporalTable();
});

modelBuilder.Entity<Address>(b => {
    b.HasTemporalTable();
});



2. DbContext Initialization
===========================
This replaces the necessary EF pipeline services to produce the new SQL syntax.

public static void Configure(DbContextOptionsBuilder<DbContext> builder, string connectionString)
{
    builder
        .UseSqlServer(connectionString)
        .EnableTemporalTableQueries(); // here we enable temporal tables inthe EF query pipeline
}


3. Querying
===========
The following is an example of querying a customer record from a Temporal Table at a specific
time, including an Address record from that same time.
All joined/projected relationships will have the "FOR SYSTEM_TIME" predicate applied to the generated SQL.

var yesterdaysCustomerRecord = _db.Customers
                                    .Include(x => x.Address)
                                    .AsOf(DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)))

Resulting SQL (pseudo-example for demonstrative purposes)

SELECT * FROM Customer FOR SYSTEM TIME AS OF '2020-02-28T11:00:00' c 
LEFT JOIN 
Address FOR SYSTEM TIME AS OF '2020-02-28T11:00:00' a 
ON c.Id = a.CustomerId