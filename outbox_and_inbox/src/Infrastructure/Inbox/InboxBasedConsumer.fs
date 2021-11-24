namespace Infrastructure.Inbox

open System
open System.Transactions

module InboxBasedConsumer =
    let consume (messageAlreadyProcessed: Guid -> Async<bool>) (saveProcessedMessageId: Guid -> Async<unit>) (func: 'a -> Async<unit>) (messageId: Guid) (message: 'a) =
        async {
            let! isProcessed = messageAlreadyProcessed (messageId)

            match isProcessed with
            | false ->
                use transactionScope =
                    new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled)

                do! func message
                do! saveProcessedMessageId (messageId)
                transactionScope.Complete()
            | true -> ()
        }