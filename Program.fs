open System
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Hosting
open HikePlanner.Views.Home
open Giraffe.EndpointRouting
open HikePlanner.Infrastructure
open HikePlanner.Core
open HikePlanner.Handlers.Handlers

let private defaultConnectionString = ConnectionString "Data Source=hikes.db"

let withApp (appEnv: AppEnv) (app: App<EnvironmentWithContext, Giraffe.ViewEngine.HtmlElements.XmlNode, Giraffe.ViewEngine.HtmlElements.XmlNode>) next ctx =  
        task {
            let environment = { Environment = appEnv; Context = ctx}
            let! result = App.run environment app

            return! match result with
                    | Ok handler -> Giraffe.Core.htmlView handler next ctx
                    | Error handler -> Giraffe.Core.htmlView handler next ctx
        }

let withAppHandler (appEnv: AppEnv) app next ctx =  
        task {
            let environment = { Environment = appEnv; Context = ctx}
            let! result = App.run environment app

            return! match result with
                    | Ok handler -> handler next ctx
                    | Error handler -> handler next ctx
        }

let endpoints env = 
    let render = withApp env
    [
        GET [
            route "/" homeHandler
            route "/plan/create" (planHandler |> render)
            route "/plan" (listPlansHandler |> render)
        ]
        POST [
            route "/plan" (withAppHandler env saveHikePlan)
        ]
    ]

[<EntryPoint>]
let main _ =
    let builder = WebApplication.CreateBuilder()

    let app = builder.Build()

    let env = {
        ConnectionString = defaultConnectionString
    }

    app.UseRouting().UseEndpoints(fun e->
        e.MapGiraffeEndpoints (endpoints env)
    ) |> ignore

    app.Run()

    0
