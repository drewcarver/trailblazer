open System
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Hosting
open HikePlanner.Views.Home
open HikePlanner.Views.Plan
open Giraffe.EndpointRouting
open HikePlanner.Infrastructure
open HikePlanner.Core
open HikePlanner.Core.Utils
open HikePlanner.Handlers.Handlers

let private defaultConnectionString = ConnectionString "Data Source=hikes.db"

let withAppCtx (env: 'env) (app: App<'env * 'ctx, 'a, 'b>) = 
    fun next (ctx: 'ctx) -> 
    task {
        let! result = App.run (env, ctx) app

        match result with
            | Ok handler -> return! handler next ctx
            | Error handler -> return! handler next ctx
    }

let withApp (env: 'env) (app: App<'env, 'a, 'b>) = (fun next ctx ->
    task {
        let! result = App.run env app

        match result with
            | Ok handler -> return! handler next ctx
            | Error handler -> return! handler next ctx
    }
)

let endpoints connectionString = 
    [
        GET [
            route "/" homeHandler
            route "/plan/create" (withApp connectionString planHandler)
            route "/plan" (listPlansHandler connectionString)
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
