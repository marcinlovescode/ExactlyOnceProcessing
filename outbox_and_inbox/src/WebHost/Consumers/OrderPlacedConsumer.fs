namespace WebHost.Consumers

open System.Threading.Tasks
open Contracts
open MassTransit
open WebHost

type OrderPlacedConsumer(compositionRoot: CompositionRoot) =
    interface IConsumer<OrderPlaced> with
        member this.Consume(context: ConsumeContext<OrderPlaced>) =
            async {
                do! compositionRoot.OrderPlacedHandler context.Message
            }
            |> Async.StartAsTask
            :> Task
