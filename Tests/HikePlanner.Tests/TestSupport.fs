namespace HikePlanner.Tests

open System
open System.Collections.Generic
open System.Net.Http
open System.Security.Claims
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.TestHost
open Microsoft.Data.Sqlite
open Giraffe.EndpointRouting
open HikePlanner.Core
open Program
open HikePlanner.Handlers.Handlers

type TestContext = {
    KeepAliveConnection: SqliteConnection
    App: WebApplication
    Client: HttpClient
}
with
    interface IDisposable with
        member this.Dispose() =
            this.App.StopAsync().GetAwaiter().GetResult()
            this.Client.Dispose()
            this.App.DisposeAsync().AsTask().GetAwaiter().GetResult()
            this.KeepAliveConnection.Dispose()

module TestSupport =
    let private testConnectionString =
        ConnectionString "Data Source=file:plan-test?mode=memory&cache=shared"

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

    let private seedDatabase (conn: SqliteConnection) =
        use cmd = conn.CreateCommand()
        cmd.CommandText <-
            """
            CREATE TABLE IF NOT EXISTS TrailPointsOfInterest (
                ID INTEGER PRIMARY KEY,
                Name TEXT NOT NULL,
                TrailName TEXT NOT NULL,
                TrailMile REAL NOT NULL
            );

            INSERT INTO TrailPointsOfInterest (TrailName, TrailMile, Name) VALUES ('AppalachianTrail', 1, 'Test Point');
            INSERT INTO TrailPointsOfInterest (TrailName, TrailMile, Name) VALUES ('AppalachianTrail', 2, 'Test Point 2');
            INSERT INTO TrailPointsOfInterest (TrailName, TrailMile, Name) VALUES ('AppalachianTrail', 3, 'Test Point 3');
            """
        cmd.ExecuteNonQuery() |> ignore

    let buildTestContext () =
        task {
            let (ConnectionString connectionString) = testConnectionString
            let keepAliveConnection = new SqliteConnection(connectionString)
            do! keepAliveConnection.OpenAsync()
            seedDatabase keepAliveConnection

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
                KeepAliveConnection = keepAliveConnection
                App = app
                Client = app.GetTestClient()
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

        new FormUrlEncodedContent(pairs :> seq<KeyValuePair<string, string>>)
