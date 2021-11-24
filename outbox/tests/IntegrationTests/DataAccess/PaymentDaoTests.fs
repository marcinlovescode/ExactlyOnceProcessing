module IntegrationTests.DataAccess.PaymentDaoTests

open Infrastructure.DataAccess
open ObjectMothers

open Xunit
open FsUnit.Xunit

[<Fact>]
let ``Persists an Order`` () =
  //Arrange
  let payment = A_Payment.``a default Payment``()

  let result =
      async {
          // Act
          do! PaymentDao.save TestDbContext.createTestDbConnection payment
          // Assert
          return! PaymentDao.readByClientId TestDbContext.createTestDbConnection payment.ClientId
      }
      |> Async.RunSynchronously

  result |> should equal [payment]