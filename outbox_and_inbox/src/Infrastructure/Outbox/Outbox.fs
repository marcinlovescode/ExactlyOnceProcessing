module Infrastructure.Outbox.Outbox

open System
open System.Reflection
open Infrastructure.Outbox.Types
open Newtonsoft.Json
open FsToolkit.ErrorHandling

[<Literal>]
let ParallelizationThreshold = 10

let add makeId readDateTimeNow save (messages: Object list) =
    let outboxMessages =
        messages
        |> List.map
            (fun message ->
                { Id = makeId ()
                  OccuredOn = readDateTimeNow ()
                  Payload = JsonConvert.SerializeObject message
                  Type = message.GetType().FullName })

    async { do! save outboxMessages }

let execute read setProcessed publish marker =
    let contractsAssembly = Assembly.GetAssembly(marker)

    let executeInternal message =
        async {
            let deserializedMessage =
                JsonConvert.DeserializeObject(message.Payload, contractsAssembly.GetType(message.Type))

            do! publish (deserializedMessage, message.Id)
            do! setProcessed message
        }


    async {
        let! chunksOfMessages =
            read ()
            |> Async.map (fun messages -> messages |> List.chunkBySize ParallelizationThreshold)

        do!
            chunksOfMessages
            |> List.map
                (fun msgChunk ->
                    msgChunk
                    |> List.map executeInternal
                    |> Async.Parallel)
            |> Async.Sequential
            |> Async.Ignore
    }
