module WebHost.HttpHandlers

open System.Transactions
open Giraffe
open Microsoft.AspNetCore.Http
open FSharp.Control.Tasks
open Application

let createTransactionScope () =
    new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled)

let placeOrderHandler : HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let compositionRoot = ctx.GetService<CompositionRoot>()
            let! command = ctx.BindJsonAsync<PlaceOrderHandler.Command>()
            use transactionScope = createTransactionScope ()
            do! compositionRoot.PlaceOrderHandler command
            transactionScope.Complete()
            return! Successful.CREATED command.Id next ctx
        }

let placeOrderHandlerFailing : HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let compositionRoot = ctx.GetService<CompositionRoot>()
            let! command = ctx.BindJsonAsync<PlaceOrderHandler.Command>()
            use transactionScope = createTransactionScope ()
            do! compositionRoot.FailingPlaceOrderHandler command
            transactionScope.Complete()
            return! Successful.CREATED command.Id next ctx
        }

let readOrderById id : HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let compositionRoot = ctx.GetService<CompositionRoot>()
            let! result = compositionRoot.ReadOrderById id

            return!
                match result with
                | Some order -> Successful.OK order next ctx
                | None -> RequestErrors.NOT_FOUND $"Not found order of id {id}" next ctx
        }

let readPaymentsByClientId id : HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let compositionRoot = ctx.GetService<CompositionRoot>()
            let! result = compositionRoot.ReadPaymentsByClientId id
            return! Successful.OK result next ctx
        }


let routing : (HttpFunc -> HttpContext -> HttpFuncResult) =
    choose [ POST
             >=> route "/orders"
             >=> placeOrderHandler
             POST
             >=> route "/failingOrders"
             >=> placeOrderHandlerFailing
             GET
             >=> routef "/orders/%O" readOrderById
             GET
             >=> routef "/client/%O/payments" readPaymentsByClientId  ]
