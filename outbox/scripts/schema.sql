IF NOT EXISTS(SELECT * 
              FROM sys.databases 
              WHERE name = 'ExactlyOnceProcessing')
    BEGIN
        CREATE DATABASE [ExactlyOnceProcessing]
    END

IF NOT EXISTS(SELECT *
              FROM sys.tables
              WHERE name = 'OutboxMessages')
CREATE TABLE OutboxMessages
(
    Id        uniqueidentifier NOT NULL,
    OccuredOn DateTime2        NOT NULL,
    Type      NVARCHAR(256)    NOT NULL,
    Payload   NVARCHAR(MAX)    NOT NULL,
    CONSTRAINT [PK_OutboxMessages_Id] PRIMARY KEY (Id)
)


IF NOT EXISTS(SELECT *
              FROM sys.tables
              WHERE name = 'ProcessedOutboxMessages')
CREATE TABLE ProcessedOutboxMessages
(
    Id          uniqueidentifier NOT NULL,
    OccuredOn   DateTime2        NOT NULL,
    Type        NVARCHAR(256)    NOT NULL,
    Payload     NVARCHAR(MAX)    NOT NULL,
    ProcessedOn DateTime2        NOT NULL
        CONSTRAINT [PK_ProcessedOutboxMessages_Id] PRIMARY KEY (Id)
)

IF NOT EXISTS(SELECT *
              FROM sys.tables
              WHERE name = 'Orders')
CREATE TABLE Orders
(
    Id          uniqueidentifier NOT NULL,
    ClientId    uniqueidentifier NOT NULL,
    TotalValue  decimal(18,4)    NOT NULL
        CONSTRAINT [PK_Orders_Id] PRIMARY KEY (Id)
)


IF NOT EXISTS(SELECT *
              FROM sys.tables
              WHERE name = 'Payments')
CREATE TABLE Payments
(
    Id          uniqueidentifier NOT NULL,
    OrderId     uniqueidentifier NOT NULL,
    ClientId    uniqueidentifier NOT NULL,
    Amount      decimal(18,4)    NOT NULL
        CONSTRAINT [PK_Payments_Id] PRIMARY KEY (Id)
)


