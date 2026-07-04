open System
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Hosting
open HikePlanner.Views.Home
open HikePlanner.Views.Plan
open HikePlanner.Repositories.HikeRepo
open Giraffe
open HikePlanner.App
open HikePlanner.Views.ListPlans

[<CLIMutable>]
type SaveHikeForm = {
    TrailName: string
    StartDate: string
    EndDate: string
}

let private tryParseDate (input: string) =
    match DateTime.TryParse input with
    | true, parsed -> Ok parsed
    | false, _ -> Error "Invalid date"

let withRouteContext (connectionString: ConnectionString) (httpHandler: Microsoft.AspNetCore.Http.HttpContext) f =
     App.run (connectionString, httpHandler) f 

let saveHikePlan: App<(ConnectionString * Microsoft.AspNetCore.Http.HttpContext), string, string> =
    app {
        let! _, ctx = App.ask
        let! form = ctx.TryBindFormAsync<SaveHikeForm>() 

        let! startDate = tryParseDate form.StartDate 
        let! endDate = tryParseDate form.EndDate 

        let! _ = 
            saveHike form.TrailName startDate endDate
            |> App.mapError (fun err ->
                match err with
                | DatabaseError msg -> sprintf "Database error: %s" msg
                | NotFound msg -> sprintf "Not found: %s" msg
            )

        return sprintf "Hike plan for %s saved successfully!" form.TrailName
    } 

let private defaultConnectionString = ConnectionString "Data Source=hikes.db"

let webAppWith connectionString =
    choose [
        route "/" >=> homeHandler
        route "/plan" >=> choose [
            //route "/create" >=> planHandler
            GET >=> fun next ctx -> 
                task {
                    let! result = App.run connectionString getSavedHikes

                    return! htmlView (listPlans result) next ctx
                } 
            (* POST >=> (fun next ctx ->  
                task {
                    let! result = App.run (connectionString, ctx) saveHikePlan

                    match result with
                    | Ok message ->
                        return! text message next ctx
                    | Error errorMsg ->
                        return! text errorMsg next ctx
                }
            )
            *)
        ]
    ]

let webApp = webAppWith defaultConnectionString

[<EntryPoint>]
let main _ =
    let builder = WebApplication.CreateBuilder()

    builder.Services.AddGiraffe() |> ignore

    let app = builder.Build()

    app.UseGiraffe webApp

    app.Run()

    0
