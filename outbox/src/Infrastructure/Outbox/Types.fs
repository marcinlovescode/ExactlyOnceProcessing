module Infrastructure.Outbox.Types

open System

type Message = {
    Id: Guid
    OccuredOn: DateTime
    Payload: string
    Type: string
}
