module ObjectMothers.A_Message

open System
open Infrastructure.Outbox.Types

let ``a default Message`` () : Message =
    { Id = Guid.NewGuid()
      OccuredOn = DateTime.UtcNow.Date
      Payload =
          An_OutboxMessage.``a default OutboxMessage`` ()
          |> Newtonsoft.Json.JsonConvert.SerializeObject
      Type = typeof<AnOutboxMessage>.FullName }
