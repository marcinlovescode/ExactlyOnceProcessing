namespace Contracts

open System

type OrderPlaced = {
    OrderId: Guid
    ClientId: Guid
    TotalValue: decimal
}


