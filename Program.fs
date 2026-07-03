open System
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Hosting
open HikePlanner.Views.Home
open HikePlanner.Views.Plan
open HikePlanner
open HikePlanner.Repositories.HikeRepo
open Giraffe
open HikePlanner.App
open HikePlanner.Repositories

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

type ConnectionString = ConnectionString of string

let withRouteContext (connectionString: ConnectionString) (httpHandler: Microsoft.AspNetCore.Http.HttpContext) f =
    (connectionString, httpHandler) |> App.run |> f

let saveHikePlan connectionString: App<(string * Microsoft.AspNetCore.Http.HttpContext), string, string> =
    app {
        let! (connectionString, ctx) = App.ask
        let! form = ctx.TryBindFormAsync<SaveHikeForm>() 

        let! startDate = tryParseDate form.StartDate 
        let! endDate = tryParseDate form.EndDate 

        let! test = 
            HikeRepo.saveHike form.TrailName startDate endDate
            |> App.mapError (fun err ->
                match err with
                | HikeRepoError.DatabaseError msg -> sprintf "Database error: %s" msg
                | HikeRepoError.NotFound msg -> sprintf "Not found: %s" msg
            )

        return sprintf "Hike plan for %s saved successfully!" form.TrailName
    } |> App.run

let private defaultConnectionString = "Data Source=hikes.db"

let webAppWith connectionString =
    choose [
        route "/" >=> homeHandler
        route "/plan" >=> choose [
            GET >=> planHandler
        ]
    ]

let webApp = webAppWith defaultConnectionString

[<EntryPoint>]
let main args =
    let builder = WebApplication.CreateBuilder(args)

    builder.Services.AddGiraffe() |> ignore

    let app = builder.Build()

    app.UseGiraffe webApp

    app.Run()

    0
