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

let render app = app |> App.map(fun v -> Giraffe.Core.htmlView v) |> App.mapError(fun v -> Giraffe.Core.htmlView v)

let endpoints connectionString = 
    [
        GET [
            route "/" homeHandler
            route "/plan/create" (withApp connectionString (planHandler |> render))
            route "/plan" (listPlansHandler connectionString)
        ]
        POST [
            // route "/plan" (withAppCtx connectionString (saveHikePlan |> render))
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
