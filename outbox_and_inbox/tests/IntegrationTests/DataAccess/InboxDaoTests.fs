module IntegrationTests.DataAccess.OutboxDaoTests

open Infrastructure.DataAccess
open Xunit
open FsUnit.Xunit

[<Fact>]
let ``Persist 100 OutboxMessages at once `` () =
    //Arrange
    let messages =
        [ 1 .. 1 .. 100 ]
        |> List.map (fun id -> ObjectMothers.A_Message.``a default Message`` ())

    let messageIds = messages |> List.map (fun msg -> msg.Id)

    let result =
        async {
            // Act
            do! OutboxDao.save TestDbContext.createTestDbConnection messages
            // Assert
            return! OutboxDao.read TestDbContext.createTestDbConnection
        }
        |> Async.RunSynchronously

    let filteredMessages =
        result
        |> List.filter (fun msg -> messageIds |> List.contains msg.Id)

    filteredMessages |> should haveLength 100

[<Fact>]
let ``Moves messages to processed table`` () =
    //Arrange
    let msg =
        ObjectMothers.A_Message.``a default Message`` ()

    let result =
        async {
            // Act
            do! OutboxDao.save TestDbContext.createTestDbConnection [ msg ]
            do! OutboxDao.moveToProcessed TestDbContext.createTestDbConnection msg

            // Assert
            let! notProcessedResult = OutboxDao.read TestDbContext.createTestDbConnection
            let! processedResult = OutboxDao.readProcessed TestDbContext.createTestDbConnection
            return (notProcessedResult, processedResult)
        }
        |> Async.RunSynchronously

    result
    |> fst
    |> Seq.filter (fun msgFromDb -> msgFromDb.Id = msg.Id)
    |> List.ofSeq
    |> should haveLength 0

    result
    |> snd
    |> Seq.filter (fun msgFromDb -> msgFromDb.Id = msg.Id)
    |> List.ofSeq
    |> should haveLength 1
