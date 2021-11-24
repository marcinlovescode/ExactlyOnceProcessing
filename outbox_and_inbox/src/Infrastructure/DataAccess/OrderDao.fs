module Infrastructure.DataAccess.OrderDao

open Domain
open FsToolkit.ErrorHandling
open Infrastructure.DapperWrapper.DapperFSharp

[<Literal>]
let private readByIdSql =
    "SELECT Id, ClientId, TotalValue FROM Orders WHERE Id = @Id"

[<Literal>]
let private insertSql =
    "INSERT INTO Orders (Id, ClientId, TotalValue) VALUES (@Id, @ClientId, @TotalValue)"

let read createConnection id =
    async {
        use! connection = createConnection ()

        return!
            connection
            |> (dbQuerySingleOrNone<Order> readByIdSql {| Id = id |})
    }

let save createConnection order =
    async {
        use! connection = createConnection ()

        do!
            connection
            |> dbParametrizedExecute insertSql order
    }