namespace ObjectMothers

open System

type AnOutboxMessage =
    { StringProperty: string
      DecimalProperty: decimal }

module An_OutboxMessage =
    let ``a default OutboxMessage`` () =
        { StringProperty = Guid.NewGuid().ToString()
          DecimalProperty = 9.99M }

    let ``with the DecimalProperty`` value anOutboxMessage =
        { anOutboxMessage with DecimalProperty = value }
