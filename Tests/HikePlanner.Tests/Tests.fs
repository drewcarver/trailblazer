module Tests

open System
open System.Collections.Generic
open System.Net
open System.Net.Http
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.TestHost
open Giraffe.EndpointRouting
open Xunit
open HikePlanner.Core
open Program
open HikePlanner.Handlers.Handlers
open Microsoft.Data.Sqlite

[<Fact>]
let ``POST /plan saves a hike through the route and SQLite`` () =
    task {
        let (ConnectionString connectionString) = ConnectionString "Data Source=file:plan-test?mode=memory&cache=shared"
        use conn = new SqliteConnection(connectionString)

        let cmd = conn.CreateCommand()
        conn.Open()
        cmd.CommandText <- """
            CREATE TABLE IF NOT EXISTS TrailPointsOfInterest (
                ID INTEGER PRIMARY KEY,
                Name TEXT NOT NULL,
                TrailName TEXT NOT NULL,
                TrailMile REAL NOT NULL
            );

            INSERT INTO TrailPointsOfInterest (TrailName, TrailMile, Name) VALUES ('Appalachian Trail', 1, "Test Point");
            INSERT INTO TrailPointsOfInterest (TrailName, TrailMile, Name) VALUES ('Appalachian Trail', 2, "Test Point 2");
        """
        cmd.ExecuteNonQuery() |> ignore

        let builder = WebApplication.CreateBuilder()
        builder.WebHost.UseTestServer() |> ignore

        let app = builder.Build()
        let env = {
            ConnectionString = ConnectionString connectionString
        }
        app.UseRouting().UseEndpoints(fun e->
            e.MapGiraffeEndpoints (endpoints env)
        ) |> ignore
        do! app.StartAsync() 

        use client = app.GetTestClient()

        let formToSave: SaveHikeForm = {
            HikeName     = "Mossy Peak"
            StartDate    = DateTime.Now
            CampPoints   = ["1"; "2"]
        }

        let form =
            new FormUrlEncodedContent(dict [
                nameof formToSave.HikeName,     formToSave.HikeName;
                nameof formToSave.StartDate,    formToSave.StartDate.ToString "yyyy-MM-dd";
                nameof formToSave.CampPoints,   formToSave.CampPoints.Head;
                nameof formToSave.CampPoints,   formToSave.CampPoints.[1];
            ])

        let! response = client.PostAsync("/plan", form)

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode)
        Assert.Equal("/plan", response.Headers.GetValues "HX-Location" |> Seq.head)
        Assert.Equal("1", response.Headers.GetValues "x-hike-id" |> Seq.head)

        let! getHikesResponse = client.GetAsync "/plan"
        let! content = getHikesResponse.Content.ReadAsStringAsync()

        Assert.Contains(formToSave.HikeName, content)
    }