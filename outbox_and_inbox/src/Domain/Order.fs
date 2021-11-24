namespace Domain

open System
open Contracts

type Order =
    { Id: Guid
      ClientId: Guid
      TotalValue: decimal }

module Orders =
    let placeOrder id clientId totalValue =
        let creatOrder id clientId totalValue =
            { Id = id
              ClientId = clientId
              TotalValue = totalValue }

        let createOrderPlacedEvent order =
            { OrderId = order.Id
              ClientId = order.ClientId
              TotalValue = order.TotalValue }

        let order = creatOrder id clientId totalValue
        (order, order |> createOrderPlacedEvent)
