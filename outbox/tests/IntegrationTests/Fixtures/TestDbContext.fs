module IntegrationTests.DataAccess.TestDbContext

open Infrastructure.DapperWrapper.DapperFSharp

[<Literal>]
let testDbConnectionString =
    "Server=localhost,1433;Initial Catalog=ExactlyOnceProcessing;User ID=sa;Password=yourStrongP@ssword;MultipleActiveResultSets=True;Connection Timeout=10;"

let createTestDbConnection =
    createSqlConnection testDbConnectionString
