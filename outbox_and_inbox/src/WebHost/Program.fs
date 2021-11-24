module WebHost.App

open System
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.DependencyInjection
open Giraffe
open Microsoft.Extensions.Logging
open WebHost.BackgroundProcessing.CronBackgroundService
open WebHost.Configuration
open WebHost.MessageQueues
open MassTransit

open WebHost.BackgroundProcessing

let configureLogging (builder: ILoggingBuilder) =

    builder.AddConsole().AddDebug() |> ignore

let errorHandler (ex: Exception) (logger: ILogger) =
    logger.LogError(EventId(), ex, "An unhandled exception has occurred while executing the request.")

    clearResponse
    >=> ServerErrors.INTERNAL_ERROR ex.Message

let private tryGetEnvironmentVariable =
    Environment.GetEnvironmentVariable
    >> function
    | null
    | "" -> None
    | variable -> Some variable

let configureSettings (configurationBuilder: IConfigurationBuilder) =
    let path = AppContext.BaseDirectory

    configurationBuilder
        .SetBasePath(path)
        .AddJsonFile("appsettings.json", false, true)
        .AddJsonFile(
            (sprintf
                $"""appsettings.{
                                     (tryGetEnvironmentVariable "ASPNETCORE_ENVIRONMENT"
                                      |> Option.toObj)
                }.json"""),
            true,
            true
        )
        .AddEnvironmentVariables()
        .Build()

let configureApp (ctx: WebHostBuilderContext) (appBuilder: IApplicationBuilder) =
    appBuilder
        .UseGiraffeErrorHandler(errorHandler)
        .UseGiraffe(HttpHandlers.routing)

let configureServices (appSettings: AppSettings) (services: IServiceCollection) =
    services
        .AddSingleton<CompositionRoot>(fun provider -> CompositionRoot.compose appSettings (provider.GetService<IBusControl>()))
        .AddMassTransit(fun cfg -> QueueListener.connect cfg appSettings.RabbitMqEndpoint appSettings.UseInbox )
        .AddHostedService(fun provider ->
            new CronBackgroundService.CronBackgroundService(
                provider.GetService<ILogger<CronBackgroundService>>(),
                "*/5 * * * * *",
                provider
                    .GetService<CompositionRoot>()
                    .OutboxProcessor
            ))
        .AddMassTransitHostedService()
        .AddGiraffe()
    |> ignore

[<EntryPoint>]
let main _ =
    async {
        let configurationRoot =
            configureSettings (ConfigurationBuilder())

        let settings = configurationRoot.Get<AppSettings>()

        Host
            .CreateDefaultBuilder()
            .ConfigureWebHostDefaults(fun webHostBuilder ->
                webHostBuilder
                    .Configure(configureApp)
                    .ConfigureServices(configureServices settings)
                    .ConfigureLogging(configureLogging)
                |> ignore)
            .Build()
            .Run()

        return 0
    }
    |> Async.RunSynchronously
