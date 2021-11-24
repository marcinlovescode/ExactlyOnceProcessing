namespace WebHost

open System
open Application
open Contracts
open Domain
open Infrastructure
open Infrastructure.DataAccess
open Infrastructure.Outbox
open MassTransit
open WebHost.Configuration


type CompositionRoot =
    { OutboxProcessor: unit -> Async<unit>
      PlaceOrderHandler: PlaceOrderHandler.Command -> Async<unit>
      FailingPlaceOrderHandler: PlaceOrderHandler.Command -> Async<unit>
      ReadOrderById: Guid -> Async<Order option>
      OrderPlacedHandler: OrderPlaced -> Async<unit>
      ReadIfMessageAlreadyProcessed: Guid -> Async<bool>
      SaveProcessedMessage: Guid -> Async<unit>
      ReadPaymentsByClientId: Guid -> Async<Payment list>
      }

module CompositionRoot =
    let buildPublisherFunc (appSettings: AppSettings) (busControl: IBusControl) =
        match appSettings.UseOutbox with
        | true ->
            Outbox.add
                (fun _ -> Guid.NewGuid())
                (fun _ -> DateTime.UtcNow)
                (appSettings.DbConnectionString
                 |> Infrastructure.DapperWrapper.DapperFSharp.createSqlConnection
                 |> OutboxDao.save)
        | false ->
            let sendEndpoint = busControl :> IPublishEndpoint

            fun messages ->
                messages
                |> List.map (fun msg -> EventPublisher.publish sendEndpoint (msg, Guid.NewGuid()))
                |> Async.Parallel
                |> Async.Ignore


    let compose (appSettings: AppSettings) (busControl: IBusControl) : CompositionRoot =
        let sendEndpoint = busControl :> IPublishEndpoint

        let publishFunc =
            buildPublisherFunc appSettings busControl

        { OutboxProcessor =
              fun () ->
                  Outbox.execute
                      (fun () ->
                          OutboxDao.read (
                              appSettings.DbConnectionString
                              |> Infrastructure.DapperWrapper.DapperFSharp.createSqlConnection
                          ))
                      (appSettings.DbConnectionString
                       |> Infrastructure.DapperWrapper.DapperFSharp.createSqlConnection
                       |> OutboxDao.moveToProcessed)
                      (fun messageWithId ->
                          messageWithId
                          |> EventPublisher.publish sendEndpoint)
                      typeof<OrderPlaced>
          PlaceOrderHandler =
              fun command ->
                  let commandHandlerIo =
                      { PlaceOrderHandler.IO.Save =
                            appSettings.DbConnectionString
                            |> Infrastructure.DapperWrapper.DapperFSharp.createSqlConnection
                            |> OrderDao.save
                        PlaceOrderHandler.IO.PublishEvent = fun event -> publishFunc [ event ] }

                  PlaceOrderHandler.handle false commandHandlerIo command
          ReadOrderById =
              appSettings.DbConnectionString
              |> Infrastructure.DapperWrapper.DapperFSharp.createSqlConnection
              |> OrderDao.read
          FailingPlaceOrderHandler =
              fun command ->
                  let commandHandlerIo =
                      { PlaceOrderHandler.IO.Save =
                            appSettings.DbConnectionString
                            |> Infrastructure.DapperWrapper.DapperFSharp.createSqlConnection
                            |> OrderDao.save
                        PlaceOrderHandler.IO.PublishEvent = fun event -> publishFunc [ event ] }

                  PlaceOrderHandler.handle true commandHandlerIo command
          OrderPlacedHandler =
              fun event ->
                  let commandHandlerIo =
                      { OrderPlacedHandler.IO.Save =
                            appSettings.DbConnectionString
                            |> Infrastructure.DapperWrapper.DapperFSharp.createSqlConnection
                            |> PaymentDao.save }

                  OrderPlacedHandler.handle commandHandlerIo event
          ReadIfMessageAlreadyProcessed = appSettings.DbConnectionString|> Infrastructure.DapperWrapper.DapperFSharp.createSqlConnection|> InboxDao.readIfExist
          SaveProcessedMessage = appSettings.DbConnectionString|> Infrastructure.DapperWrapper.DapperFSharp.createSqlConnection|> InboxDao.save
          ReadPaymentsByClientId = appSettings.DbConnectionString|> Infrastructure.DapperWrapper.DapperFSharp.createSqlConnection|> PaymentDao.readByClientId }
