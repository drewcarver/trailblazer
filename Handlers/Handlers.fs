namespace HikePlanner.Handlers

open HikePlanner.Infrastructure
open HikePlanner.Repositories.HikeRepo
open HikePlanner.Core
open HikePlanner.Views.Plan
open HikePlanner.Views.Plan.ListPlans
open System
open Giraffe
open Giraffe.ViewEngine
open Utils

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

    let listPlansHandler: TrailblazerEndpoint<_> =
        app {
            let! hikes = getSavedHikes

            return (hikes
                |> Ok
                |> listPlans
                |> htmlView)
        } 
        |> App.mapError (Error >> listPlans >> htmlView)

    let planHandler: TrailblazerEndpoint<_> =
        app {
            let! plans = getTrailPointsOfInterest "AppalachianTrail"

            return htmlView (Plan.planView (Ok plans))
        } |> App.mapError (fun e -> htmlView (Plan.planView (Error e)))

    let saveHikePlan : TrailblazerEndpoint<_> =
        app {
            let! { Context = ctx } = App.ask

            let! form: SaveHikeForm = getFormHelper ctx 

            let! _ = saveHike form.HikeName form.StartDate form.EndDate 

            return! App.succeed (redirectTo false "/plan")
        } 
        |> App.mapError (fun e -> htmlView (Plan.planView (Error e)))
