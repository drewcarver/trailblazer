module Tests

open System
open System.Collections.Generic
open System.Net
open System.Net.Http
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.TestHost
open Microsoft.Data.Sqlite
open Giraffe
open Xunit
open Program

[<Fact>]
let ``POST /plan saves a hike through the route and SQLite`` () =
    let connectionString = "Data Source=file:plan-test?mode=memory&cache=shared"

    let builder = WebApplication.CreateBuilder()
    builder.WebHost.UseTestServer() |> ignore
    builder.Services.AddGiraffe() |> ignore

    let app = builder.Build()
    app.UseGiraffe (Program.webAppWith (ConnectionString connectionString))
    app.StartAsync() |> ignore

    use client = app.GetTestClient()

    let form =
        new FormUrlEncodedContent(
            [ KeyValuePair<string, string>("TrailName", "Mossy Peak")
              KeyValuePair<string, string>("StartDate", "2026-06-28")
              KeyValuePair<string, string>("EndDate", "2026-06-29") ])

    let response = client.PostAsync("/plan", form).Result

    Assert.Equal(HttpStatusCode.OK, response.StatusCode)
    let body = response.Content.ReadAsStringAsync().Result
    Assert.Contains("Mossy Peak", body)

    use conn = new SqliteConnection(connectionString)
    conn.Open()

    use cmd = conn.CreateCommand()
    cmd.CommandText <- "SELECT COUNT(*) FROM hike"
    let count =
        match cmd.ExecuteScalar() with
        | :? int64 as id -> id
        | :? int32 as i -> int64 i
        | :? int16 as i -> int64 i
        | :? string as s -> Int64.Parse s
        | _ -> 0L

    Assert.Equal(1L, count)
