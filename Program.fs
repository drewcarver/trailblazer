open System
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Hosting
open HikePlanner.Views.Home
open Giraffe.EndpointRouting
open HikePlanner.Infrastructure
open HikePlanner.Core
open HikePlanner.Handlers.Handlers

let private defaultConnectionString = ConnectionString "Data Source=hikes.db"

let withAppHandler (appEnv: 'env) app next ctx =  
        task {
            let environment = { Environment = appEnv; Context = ctx}
            let! result = App.run environment app

            return! match result with
                    | Ok handler -> handler next ctx
                    | Error handler -> handler next ctx
        }

let endpoints env = 
    [
        GET [
            route "/" homeHandler
            route "/plan/create" (withAppHandler env planHandler)
            route "/plan" (withAppHandler env listPlansHandler)
            routef "/plan/%d:id" (viewHikeHandler >> withAppHandler env)
        ]
        POST [
            route "/plan" (withAppHandler env saveHikePlan)
        ]
    ]

[<EntryPoint>]
let main _ =
    let builder = WebApplication.CreateBuilder()

    let app = builder.Build()

    app.UseStaticFiles() |> ignore

    let env = {
        ConnectionString = defaultConnectionString
    }

    app.UseRouting().UseEndpoints(fun e->
        e.MapGiraffeEndpoints (endpoints env)
    ) |> ignore

    app.Run()

    0
