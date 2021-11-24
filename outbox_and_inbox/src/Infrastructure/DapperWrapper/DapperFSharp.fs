module Infrastructure.DapperWrapper.DapperFSharp

open System.Data
open System.Data.SqlClient
open Dapper
open FsToolkit.ErrorHandling

let dbQuery<'Result> (query: string) (connection: IDbConnection) : Async<'Result seq> =
    connection.QueryAsync<'Result>(query)
    |> Async.AwaitTask

let dbParametrizedQuerySingle<'Result> (query: string) (param: obj) (connection: SqlConnection) : Async<'Result> =
    connection.QuerySingleAsync<'Result>(query, param)
    |> Async.AwaitTask

let dbParametrizedQuery<'Result> (query: string) (param: obj) (connection: SqlConnection) : Async<'Result seq> =
    connection.QueryAsync<'Result>(query, param)
    |> Async.AwaitTask

let dbParametrizedExecute (sql: string) (param: obj) (connection: SqlConnection) =
    connection.ExecuteAsync(sql, param)
    |> Async.AwaitTask
    |> Async.Ignore

let dbQuerySingleOrNone<'Result> (query: string) (param: obj) (connection: SqlConnection) : Async<'Result option> =
    connection.QueryAsync<'Result>(query, param)
    |> Async.AwaitTask
    |> Async.map Seq.tryHead

let createSqlConnection (connectionString: string) : unit -> Async<SqlConnection> =
    fun () ->
        async {
            let connection = new SqlConnection(connectionString)

            if connection.State <> ConnectionState.Open then
                do! connection.OpenAsync() |> Async.AwaitTask

            return connection
        }
