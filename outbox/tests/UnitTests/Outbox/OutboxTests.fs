module UnitTests.Outbox.OutboxTests

open System
open ObjectMothers
open Xunit
open FsUnit.Xunit
open FsToolkit.ErrorHandling

open Infrastructure.Outbox.Types
open ObjectMothers.An_OutboxMessage

[<Fact>]
let ``puts a serialized message through the save func to the outbox`` () =
    // Arrange
    let aMessage =
        An_OutboxMessage.``a default OutboxMessage`` ()

    let expectedDateTime = DateTime.UtcNow
    let expectedMsgGuid = Guid.NewGuid()
    let mutable savedMessage : Message list option = None
    let readDateTimeNowFun = fun () -> expectedDateTime
    let makeId = fun () -> expectedMsgGuid

    let saveFun =
        fun messages ->
            savedMessage <- messages |> Some
            () |> async.Return
    // Act
    Infrastructure.Outbox.Outbox.add makeId readDateTimeNowFun saveFun [ aMessage ]
    |> Async.RunSynchronously
    //Assert
    let messageFromOutbox = savedMessage |> Option.get |> List.head

    let deserializedMessage =
        messageFromOutbox.Payload
        |> Newtonsoft.Json.JsonConvert.DeserializeObject<AnOutboxMessage>

    messageFromOutbox.Id
    |> should equal expectedMsgGuid

    messageFromOutbox.OccuredOn
    |> should equal expectedDateTime

    messageFromOutbox.Type
    |> should equal typeof<AnOutboxMessage>.FullName

    deserializedMessage |> should equal aMessage

[<Fact>]
let ``publishes all of read messages and sets as processed`` () =
    //Arrange
    let outboxMessages =
        [ 1 .. 101 ]
        |> List.map
            (fun index ->
                An_OutboxMessage.``a default OutboxMessage`` ()
                |> An_OutboxMessage.``with the DecimalProperty`` (decimal index))

    let messages =
        outboxMessages
        |> List.map
            (fun msg ->
                { Id = Guid.NewGuid()
                  Type = typeof<AnOutboxMessage>.FullName
                  Payload = msg |> Newtonsoft.Json.JsonConvert.SerializeObject
                  OccuredOn = DateTime.Now })

    let mutable messagesSetAsProcessed : Message list = []
    let mutable publishedMessages : obj list = []
    let readOutboxMessagesFunc = fun () -> messages |> async.Return

    let setAsProcessedFunc =
        fun message ->
            messagesSetAsProcessed <- message :: messagesSetAsProcessed
            () |> async.Return

    let publishFunc =
        fun message ->
            let msg,_ = message
            publishedMessages <- msg :: publishedMessages
            () |> async.Return
    //Act
    Infrastructure.Outbox.Outbox.execute readOutboxMessagesFunc setAsProcessedFunc publishFunc typeof<AnOutboxMessage>
    |> Async.RunSynchronously
    //Assert
    messagesSetAsProcessed
    |> List.iter (fun msg -> messages |> should contain msg)

    publishedMessages
    |> List.iter (fun msg -> outboxMessages |> should contain msg)
