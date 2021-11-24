module WebHost.MessageQueues.QueueListener


open Contracts
open MassTransit
open MassTransit.ExtensionsDependencyInjectionIntegration
open MassTransit.RabbitMqTransport
open WebHost
open WebHost.Consumers

let connect (serviceCollectionBusConfigurator: IServiceCollectionBusConfigurator) (connectionString: string) =
    serviceCollectionBusConfigurator.UsingRabbitMq
        (fun busRegistrationContext rabbitMqBusFactoryConfigurator ->
            rabbitMqBusFactoryConfigurator.Host(connectionString)

            rabbitMqBusFactoryConfigurator.ReceiveEndpoint(
                "Contracts:OrderPlaced",
                (fun (configurator: IRabbitMqReceiveEndpointConfigurator) ->
                    configurator.Consumer<OrderPlacedConsumer>
                        (fun _ ->
                            let service =
                                busRegistrationContext.GetService<CompositionRoot>()

                            OrderPlacedConsumer(service)))
            ))
