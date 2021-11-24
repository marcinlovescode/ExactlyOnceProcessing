module WebHost.Configuration
[<CLIMutable>]
type AppSettings = {
    DbConnectionString: string
    RabbitMqEndpoint: string
    UseOutbox: bool
    UseInbox: bool
}