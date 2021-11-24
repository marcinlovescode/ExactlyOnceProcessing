namespace WebHost.Consumers

open System
open System.Threading.Tasks
open Infrastructure.Inbox
open MassTransit

type MassTransitInboxBasedConsumer<'a when 'a: not struct>(messageAlreadyProcessed: Guid -> Async<bool>, saveProcessedMessageId: Guid -> Async<unit>, func: 'a -> Async<unit>) =
    interface IConsumer<'a> with
        member this.Consume(context: ConsumeContext<'a>) =
            InboxBasedConsumer.consume messageAlreadyProcessed saveProcessedMessageId func (context.MessageId.GetValueOrDefault()) context.Message
            |> Async.StartAsTask
            :> Task
