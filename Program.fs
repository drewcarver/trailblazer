open System
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Hosting
open HikePlanner.Views.Home
open HikePlanner.Views.Plan
open HikePlanner
open HikePlanner.Repositories.HikeRepo
open Giraffe

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

let savePlanHandlerWith : TaskReader<string, HttpHandler> =
    taskReader {
        let! connectionString = TaskReader.ask

        return
            fun next ctx ->
                task {
                    let! form = ctx.TryBindFormAsync<SaveHikeForm>()

                    match form with
                    | Ok f ->
                        match tryParseDate f.StartDate, tryParseEndDate f.EndDate with
                        | Ok startDate, Ok endDate ->
                            let savedId = saveHike f.TrailName startDate endDate |> Reader.run connectionString
                            return! text $"Hike '{f.TrailName}' saved from {f.StartDate} to {f.EndDate} (id {savedId})" next ctx
                        | Error e, _
                        | _, Error e ->
                            return! text ("Invalid form data: " + e) next ctx
                    | Error e -> return! text ("Invalid form data: " + e) next ctx
                }
    }

let private defaultConnectionString = "Data Source=hikes.db"

let webAppWith connectionString =
    choose [
        route "/" >=> homeHandler
        route "/plan" >=> choose [
            GET >=> planHandler
            POST >=> (fun next ctx -> task {
                let! handler = savePlanHandlerWith |> TaskReader.run connectionString
                return! handler next ctx
            })
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
