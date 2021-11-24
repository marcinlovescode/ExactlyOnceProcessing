module Infrastructure.DataAccess.InboxDao

open FsToolkit.ErrorHandling
open Infrastructure.DapperWrapper.DapperFSharp

[<Literal>]
let private readSql =
    "SELECT COUNT(1) FROM ProcessedInboxMessages WHERE Id = @Id"

[<Literal>]
let private insertSql =
    "INSERT INTO ProcessedInboxMessages(Id) VALUES (@Id)"

let readIfExist createConnection id =
    async {
        use! connection = createConnection ()

        return!
            connection
            |> (dbParametrizedQuerySingle<int> readSql {|Id = id|})
            |> Async.map (fun count -> count = 1)
    }

let save createConnection msgId =
    async {
        use! connection = createConnection ()

        do!
            connection
            |> dbParametrizedExecute insertSql {|Id = msgId|}
    }
