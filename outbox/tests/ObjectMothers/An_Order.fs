module ObjectMothers.An_Order

open System
open Domain

let ``a default Order`` () : Order =
    { Id = Guid.NewGuid()
      ClientId = Guid.NewGuid()
      TotalValue = decimal DateTime.UtcNow.Millisecond }
