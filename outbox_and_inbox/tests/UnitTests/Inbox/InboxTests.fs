module UnitTests.Inbox.InboxTests

open System
open FsToolkit.ErrorHandling
open Xunit
open FsUnit.Xunit

[<Fact>]
let ``doesn't execute func when message already processed`` () =
    //Arrange
    let mutable funcProcessed = false
    let messageId = Guid.NewGuid()
    let messagePayload = Guid.NewGuid().ToString()

    let checkIfMessageIsAlreadyProcessedFunc =
        fun _ -> true |> async.Return

    let saveProcessedMessageIdFunc = fun _ -> () |> async.Return

    let processFunc =
        fun _ ->
            funcProcessed <- true
            () |> async.Return

    //Act
    Infrastructure.Inbox.InboxBasedConsumer.consume checkIfMessageIsAlreadyProcessedFunc saveProcessedMessageIdFunc processFunc messageId messagePayload
    |> Async.RunSynchronously
    //Assert
    funcProcessed |> should equal false

[<Fact>]
let ``executes func and saves messageId when message not processed yet`` () =
    //Arrange
    let mutable funcProcessed = false
    let mutable savedMessageId = None

    let messageId = Guid.NewGuid()
    let messagePayload = Guid.NewGuid().ToString()

    let checkIfMessageIsAlreadyProcessedFunc =
        fun _ -> false |> async.Return


    let saveProcessedMessageIdFunc = fun id ->
        savedMessageId <- id |> Some
        () |> async.Return

    let processFunc =
        fun _ ->
            funcProcessed <- true
            () |> async.Return

    //Act
    Infrastructure.Inbox.InboxBasedConsumer.consume checkIfMessageIsAlreadyProcessedFunc saveProcessedMessageIdFunc processFunc messageId messagePayload
    |> Async.RunSynchronously
    //Assert
    funcProcessed |> should equal true
    savedMessageId |> should equal (messageId |> Some)
