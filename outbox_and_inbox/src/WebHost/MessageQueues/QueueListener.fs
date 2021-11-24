module WebHost.MessageQueues.QueueListener


open Contracts
open MassTransit
open MassTransit.ExtensionsDependencyInjectionIntegration
open MassTransit.RabbitMqTransport
open WebHost
open WebHost.Consumers

let connect (serviceCollectionBusConfigurator: IServiceCollectionBusConfigurator) (connectionString: string) useInbox =
    serviceCollectionBusConfigurator.UsingRabbitMq
        (fun busRegistrationContext rabbitMqBusFactoryConfigurator ->
            rabbitMqBusFactoryConfigurator.Host(connectionString)

            rabbitMqBusFactoryConfigurator.ReceiveEndpoint(
                "Contracts:OrderPlaced",
                (fun (configurator: IRabbitMqReceiveEndpointConfigurator) ->
                    match useInbox with
                    | true ->
                        configurator.Consumer<MassTransitInboxBasedConsumer<OrderPlaced>>
                            (fun _ ->
                                let service =
                                    busRegistrationContext.GetService<CompositionRoot>()

                                MassTransitInboxBasedConsumer<OrderPlaced>(service.ReadIfMessageAlreadyProcessed, service.SaveProcessedMessage, service.OrderPlacedHandler))
                    | false ->
                        configurator.Consumer<OrderPlacedConsumer>
                            (fun _ ->
                                let service =
                                    busRegistrationContext.GetService<CompositionRoot>()

                                OrderPlacedConsumer(service)))
            ))