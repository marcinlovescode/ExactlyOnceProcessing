module Application.OrderPlacedHandler

open System
open Contracts
open Domain

type IO =
    {
        Save: Payment -> Async<unit>
    }

let handle io (event: OrderPlaced) =
    let paymentId = Guid.NewGuid()
    let payment = Payments.chargeCreditCard paymentId event.OrderId event.ClientId event.TotalValue
    payment |> io.Save