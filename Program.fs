open System
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Hosting
open HikePlanner.Views.Home
open HikePlanner.Views.Plan
open Giraffe.EndpointRouting
open HikePlanner.App
open HikePlanner.Utilities.Utilities
open HikePlanner.Handlers.Handlers

let private defaultConnectionString = ConnectionString "Data Source=hikes.db"

let endpoints connectionString = 
    [
        GET [
            route "/" homeHandler
            route "/plan/create" planHandler
            route "/plan" (listPlansHandler connectionString )
        ]
        POST [
            route "/plan" (fun next ctx ->  
                task {
                    let! result = App.run (connectionString, ctx) saveHikePlan

                    match result with
                    | Ok _ ->
                        return! Giraffe.Core.redirectTo false "/plan" next ctx
                    | Error errorMsg ->
                        return! Giraffe.Core.text errorMsg next ctx
                }
            )
        ]
    ]

[<EntryPoint>]
let main _ =
    let builder = WebApplication.CreateBuilder()

    let app = builder.Build()

    app.UseRouting().UseEndpoints(fun e->
        e.MapGiraffeEndpoints (endpoints defaultConnectionString)
    ) |> ignore

    app.Run()

    0
