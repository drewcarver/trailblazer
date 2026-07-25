namespace HikePlanner.Tests

open System
open System.Collections.Generic
open System.Net.Http
open System.Security.Claims
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.TestHost
open Giraffe.EndpointRouting
open HikePlanner.Core
open Program
open HikePlanner.Handlers.Handlers

type TestContext = {
    DbPath: string
    App: WebApplication
    Client: HttpClient
}
with
    interface IDisposable with
        member this.Dispose() =
            this.App.StopAsync().GetAwaiter().GetResult()
            this.Client.Dispose()
            this.App.DisposeAsync().AsTask().GetAwaiter().GetResult()
            if IO.File.Exists this.DbPath then
                IO.File.Delete this.DbPath

module TestSupport =
    open System.Data.Common
    open Microsoft.Data.Sqlite

    let private testUser =
        ClaimsPrincipal(
            ClaimsIdentity(
                [
                    Claim(ClaimTypes.Name, "Test User")
                    Claim(ClaimTypes.Email, "test@example.com")
                ],
                "Test"
            )
        )

    let private seedDatabase (conn: DbConnection) =
        let exec sql =
            use cmd = conn.CreateCommand()
            cmd.CommandText <- sql
            cmd.ExecuteNonQuery() |> ignore

        exec """
            CREATE TABLE IF NOT EXISTS TrailPointsOfInterest (
                ID INTEGER PRIMARY KEY,
                Name TEXT NOT NULL,
                TrailName TEXT NOT NULL,
                TrailMile REAL NOT NULL
            );"""

        exec "INSERT INTO TrailPointsOfInterest (TrailName, TrailMile, Name) VALUES ('AppalachianTrail', 1, 'Test Point');"
        exec "INSERT INTO TrailPointsOfInterest (TrailName, TrailMile, Name) VALUES ('AppalachianTrail', 2, 'Test Point 2');"
        exec "INSERT INTO TrailPointsOfInterest (TrailName, TrailMile, Name) VALUES ('AppalachianTrail', 3, 'Test Point 3');"

    let buildTestContext () =
        task {
            let connectionString = sprintf "Data Source=:memory:"

            use initConn = new SqliteConnection(connectionString)
            do! initConn.OpenAsync()
            seedDatabase initConn

            let builder = WebApplication.CreateBuilder()
            builder.WebHost.UseTestServer() |> ignore

            let app = builder.Build()
            let env = { ConnectionString = ConnectionString connectionString }

            app.Use(
                Func<HttpContext, RequestDelegate, Threading.Tasks.Task>(fun ctx next ->
                    task {
                        ctx.User <- testUser
                        return! next.Invoke(ctx)
                    })
            )
            |> ignore

            app.UseRouting().UseEndpoints(fun e -> e.MapGiraffeEndpoints(endpoints env)) |> ignore
            do! app.StartAsync()

            return {
                App = app
                Client = app.GetTestClient()
                DbPath = ""
            }
        }

    let buildSaveHikeFormContent (form: SaveHikeForm) =
        let pairs = ResizeArray<KeyValuePair<string, string>>()

        pairs.Add(KeyValuePair(nameof form.HikeName, form.HikeName))
        pairs.Add(KeyValuePair(nameof form.StartDate, form.StartDate.ToString("yyyy-MM-dd")))

        for campPoint in form.CampPoints do
            pairs.Add(KeyValuePair(nameof form.CampPoints, campPoint))

        for invitee in form.Invitees do
            pairs.Add(KeyValuePair(nameof form.Invitees, invitee))

        if form.Invitees.IsEmpty then
            pairs.Add(KeyValuePair(nameof form.Invitees, ""))

        new FormUrlEncodedContent(pairs :> seq<KeyValuePair<string, string>>)
