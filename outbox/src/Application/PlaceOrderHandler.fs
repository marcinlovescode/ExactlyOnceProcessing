module Application.PlaceOrderHandler

open System
open Contracts
open Domain

type Command =
    { Id: Guid
      ClientId: Guid
      TotalValue: decimal }

type IO =
    { Save: Order -> Async<unit>
      PublishEvent: OrderPlaced -> Async<unit> }

let handle shouldFailAfterSave io command =
    let order, orderPlacedEvent =
        Orders.placeOrder command.Id command.ClientId command.TotalValue

    async {
        do! orderPlacedEvent |> io.PublishEvent

        match shouldFailAfterSave with
        | true -> failwith "ERROR!"
        | false -> do! order |> io.Save
    }
