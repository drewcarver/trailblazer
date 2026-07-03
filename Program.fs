open System
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Hosting
open HikePlanner.Views.Home
open HikePlanner.Views.Plan
open HikePlanner
open HikePlanner.Repositories.HikeRepo
open Giraffe
open HikePlanner.App

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

let private tryParseEndDate (input: string) =
    match String.IsNullOrWhiteSpace input with
    | true -> Ok None
    | false ->
        match DateTime.TryParse input with
        | true, parsed -> Ok (Some parsed)
        | false, _ -> Error "Invalid date"

let saveHikePlan connectionString: HttpHandler =
    fun next ctx ->
        task {
            let! form = ctx.TryBindFormAsync<SaveHikeForm>()
            return! text "hi" next ctx
        }

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
