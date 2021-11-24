module IntegrationTests.DataAccess.OrderDaoTests

open Infrastructure.DataAccess
open ObjectMothers

open Xunit
open FsUnit.Xunit

[<Fact>]
let ``Persists an Order`` () =
  //Arrange
  let order = An_Order.``a default Order``()

  let result =
      async {
          // Act
          do! OrderDao.save TestDbContext.createTestDbConnection order
          // Assert
          return! OrderDao.read TestDbContext.createTestDbConnection order.Id
      }
      |> Async.RunSynchronously

  result |> Option.get |> should equal order