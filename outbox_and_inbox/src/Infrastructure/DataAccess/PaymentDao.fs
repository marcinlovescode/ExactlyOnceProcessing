module Infrastructure.DataAccess.PaymentDao

open Domain
open FsToolkit.ErrorHandling
open Infrastructure.DapperWrapper.DapperFSharp

[<Literal>]
let private readByClientIdSql =
    "SELECT Id, OrderId, ClientId, Amount FROM Payments WHERE ClientId = @Id"

[<Literal>]
let private insertSql =
    "INSERT INTO Payments (Id, OrderId, ClientId, Amount) VALUES (@Id, @OrderId, @ClientId, @Amount)"

let readByClientId createConnection id =
    async {
        use! connection = createConnection ()

        return!
            connection
            |> (dbParametrizedQuery<Payment> readByClientIdSql {| Id = id |})
            |> Async.map (Seq.toList)
    }

let save createConnection order =
    async {
        use! connection = createConnection ()

        do!
            connection
            |> dbParametrizedExecute insertSql order
    }