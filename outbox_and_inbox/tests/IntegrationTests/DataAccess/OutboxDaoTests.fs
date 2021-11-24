module IntegrationTests.DataAccess.InboxDaoTests

open System
open Infrastructure.DataAccess
open Xunit
open FsUnit.Xunit

[<Fact>]
let ``Persist MessageId `` () =
    //Arrange
    let messageId = Guid.NewGuid()

    let result =
        async {
            // Act
            do! InboxDao.save TestDbContext.createTestDbConnection messageId
            // Assert
            return! InboxDao.readIfExist TestDbContext.createTestDbConnection  messageId
        }
        |> Async.RunSynchronously

    result |> should equal true
