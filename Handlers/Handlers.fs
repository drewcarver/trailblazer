namespace HikePlanner.Handlers

open HikePlanner.Infrastructure
open HikePlanner.Repositories.HikeRepo
open HikePlanner.Core
open Giraffe
open HikePlanner.Core.Utils
open HikePlanner.Views.Plan
open HikePlanner.Views.Plan.ListPlans
open HikePlanner.Core

module Handlers = 
    open Giraffe.ViewEngine
    open System
    [<CLIMutable>]
    type SaveHikeForm = {
        TrailName: string
        StartDate: string
        EndDate: string
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

    let saveHikePlan: App<EnvironmentWithContext, XmlNode, XmlNode> =
        let hike = app {
            let! { Context = ctx } = App.ask

            let! form = ctx.TryBindFormAsync<SaveHikeForm>() 
            let! startDate = tryParseDate form.StartDate 
            let! endDate = tryParseDate form.EndDate 

            return { name = form.TrailName; startDate = startDate; endDate = endDate }
        } 
        app {
            let! hike = hike |> App.mapError (fun e -> div [] [ str e ])

            return! saveHike hike.name hike.startDate hike.endDate 
                |> showStandardError
                |> App.map (fun _ -> div [] [ str "Saved" ])
        } 

    let listPlansHandler =
        app {
            let! hikes = getSavedHikes

            return ListPlans.listPlans hikes
        } |> showStandardError

    let planHandler =
        app {
            let! plans = getTrailPointsOfInterest "AppalachianTrail"

            return Plan.planView plans
        } |> showStandardError