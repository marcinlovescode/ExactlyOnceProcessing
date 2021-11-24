module Infrastructure.EventPublisher

open System
open MassTransit

let publish (publishEndpoint: IPublishEndpoint) (data: 'a, eventId: Guid) =
    let assignMessageId (publishContext: PublishContext) = publishContext.MessageId <- eventId

    publishEndpoint.Publish(data, assignMessageId)
    |> Async.AwaitTask
