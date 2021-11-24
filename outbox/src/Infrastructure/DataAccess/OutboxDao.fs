module Infrastructure.DataAccess.OutboxDao

open Infrastructure.Outbox.Types
open FsToolkit.ErrorHandling
open Infrastructure.DapperWrapper.DapperFSharp


[<Literal>]
let private readSql =
    "SELECT Id, OccuredOn as OccuredOn, Payload, Type FROM OutboxMessages"

[<Literal>]
let private readProcessedSql =
    "SELECT Id, OccuredOn as OccuredOn, Payload, Type FROM ProcessedOutboxMessages"

[<Literal>]
let private insertSql =
    "INSERT INTO OutboxMessages(Id, OccuredOn, Payload, Type) VALUES (@Id, @OccuredOn, @Payload, @Type)"

[<Literal>]
let private moveToProcessedSql = "DELETE FROM OutboxMessages
      OUTPUT DELETED.Id, DELETED.OccuredOn, DELETED.Payload, DELETED.Type, GETUTCDATE()
      INTO ProcessedOutboxMessages (Id, OccuredOn, Payload, Type, ProcessedOn)
      WHERE Id = @id"

let read createConnection =
    async {
        use! connection = createConnection ()

        return!
            connection
            |> (dbQuery<Message> readSql)
            |> Async.map (List.ofSeq)
    }

let readProcessed createConnection =
    async {
        use! connection = createConnection ()
        return! connection |> dbQuery<Message> readProcessedSql
    }

let save createConnection outboxMessages =
    async {
        use! connection = createConnection ()

        do!
            connection
            |> dbParametrizedExecute insertSql outboxMessages
    }

let moveToProcessed createConnection outboxMessage =
    async {
        use! connection = createConnection ()

        do!
            connection
            |> dbParametrizedExecute moveToProcessedSql {| id = outboxMessage.Id |}
    }
