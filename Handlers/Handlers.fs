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


    let saveHikePlan: App<EnvironmentWithContext, XmlNode, XmlNode> =
        app {
            let! { Context = ctx } = App.ask

            let! form: SaveHikeForm = ctx.TryBindFormAsync<SaveHikeForm>() |> App.ofTaskResult |> App.mapError (fun e -> div [] [])

            return! App.succeed (div [] [])

            // let! saved = saveHike hike hike.startDate hike.endDate 
            //     |> showStandardError
            //     |> App.map (fun _ -> div [] [ str "Saved" ])
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