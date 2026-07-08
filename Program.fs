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
open Microsoft.AspNetCore.Http

let private defaultConnectionString = ConnectionString "Data Source=hikes.db"

let withApp (appEnv: AppEnv) (app: App<EnvironmentWithContext, Giraffe.ViewEngine.HtmlElements.XmlNode, Giraffe.ViewEngine.HtmlElements.XmlNode>) next ctx =  
        task {
            let environment = { Environment = appEnv; Context = ctx}
            let! result = App.run environment app

            return! match result with
                    | Ok handler -> Giraffe.Core.htmlView handler next ctx
                    | Error handler -> Giraffe.Core.htmlView handler next ctx
        }


// let render a: App<EnvironmentWithContext, Giraffe.ViewEngine.HtmlElements.XmlNode, Giraffe.ViewEngine.HtmlElements.XmlNode> = 
//     a
//         |> App.map(fun v -> Giraffe.Core.htmlView v) 
//         |> App.mapError(fun v -> Giraffe.Core.htmlView v)
    

let endpoints env = 
    [
        GET [
            route "/" homeHandler
            route "/plan/create" (withApp env planHandler)
            route "/plan" (withApp env listPlansHandler)
        ]
        POST [
            route "/plan" (withApp env saveHikePlan)
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
