namespace Domain

open System
open Contracts

type Payment =
    { Id: Guid
      OrderId: Guid
      ClientId: Guid
      Amount: decimal }

module Payments =
    let chargeCreditCard id orderId clientId amount =
        { Id = id
          OrderId = orderId
          ClientId = clientId
          Amount = amount }