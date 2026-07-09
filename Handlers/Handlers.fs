namespace HikePlanner.Handlers

open HikePlanner.Infrastructure
open HikePlanner.Repositories.HikeRepo
open HikePlanner.Core
open Giraffe
open HikePlanner.Core.Utils
open HikePlanner.Views.Plan
open HikePlanner.Views.Plan.ListPlans
open System
open Giraffe.ViewEngine

module Handlers = 
    open Microsoft.AspNetCore.Http

    [<CLIMutable>]
    type SaveHikeForm = {
        HikeName: string
        StartDate: DateTime
        EndDate: DateTime
        StartPointId: int
        EndPointId: int
    }

    let showStandardError app = 
        App.mapError (fun err ->
            match err with
            | DatabaseError msg -> div [] [ str (sprintf "Database error: %s" msg) ]
            | NotFound msg -> div [] [ str (sprintf "Not found: %s" msg) ]
        ) app

    type Hike = {
        name: string
        startDate: DateTime
        endDate: DateTime
    }

    let getFormHelper<'T> (ctx: HttpContext) =
        app {
            let result = ctx.TryBindFormAsync<'T>() |> App.ofTaskResult

            return result
        }

    let listPlansHandler =
        app {
            let! hikes = getSavedHikes

            return listPlans hikes
        } |> showStandardError

    let planHandler =
        app {
            let! plans = getTrailPointsOfInterest "AppalachianTrail"

            return Plan.planView (Ok plans)
        } |> App.mapError (fun e -> Plan.planView (Error e))

    let saveHikePlan : App<EnvironmentWithContext, HttpHandler, HttpHandler> =
        app {
            let! { Context = ctx } = App.ask

            let! form: SaveHikeForm = ctx.TryBindFormAsync<SaveHikeForm>() |> App.ofTaskResult |> App.mapError (fun e -> FormValidationError e)

            return! saveHike form.HikeName form.StartDate form.EndDate
        } 
        |> App.bind (fun _ -> App.succeed (redirectTo false "/plan"))
        |> App.mapError (fun e -> htmlView (Plan.planView (Error e)))
