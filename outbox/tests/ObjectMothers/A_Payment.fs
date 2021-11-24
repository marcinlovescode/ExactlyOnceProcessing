module ObjectMothers.A_Payment

open System
open Domain

let ``a default Payment`` () : Payment =
    { Id = Guid.NewGuid()
      OrderId = Guid.NewGuid()
      ClientId = Guid.NewGuid()
      Amount = decimal DateTime.UtcNow.Second  }