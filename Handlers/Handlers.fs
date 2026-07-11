namespace HikePlanner.Handlers

open HikePlanner.Infrastructure
open HikePlanner.Repositories.HikeRepo
open HikePlanner.Core
open HikePlanner.Views.Plan
open HikePlanner.Views.Plan.ListPlans
open HikePlanner.Views.Plan.HikeDetail
open System
open Giraffe
open Utils

module Handlers = 
    [<CLIMutable>]
    type SaveHikeForm = {
        HikeName: string
        StartDate: DateTime
        EndDate: DateTime
        StartPointId: int
        EndPointId: int
    }

    let listPlansHandler: TrailblazerEndpoint<_> =
        getSavedHikes 
        |> App.map (Ok >> listPlans >> htmlView)
        |> App.mapError (Error >> listPlans >> htmlView)

    let planHandler: TrailblazerEndpoint<_> =
        getTrailPointsOfInterest "AppalachianTrail" 
        |> App.map (Ok >> Plan.planView >> htmlView)
        |> App.mapError (Error >> Plan.planView >> htmlView) 

    let saveHikePlan : TrailblazerEndpoint<_> =
        app {
            let! { Context = ctx } = App.ask

            let! form: SaveHikeForm = getFormHelper ctx 

            let! _ = saveHike form.HikeName form.StartDate form.EndDate form.StartPointId form.EndPointId

            return! App.succeed (redirectTo false "/plan")
        } 
        |> App.mapError (Error >> Plan.planView >> htmlView) 

    let viewHikeHandler hikeId : TrailblazerEndpoint<_> =
       getHikeById hikeId
        |> App.map (Ok >> hikeDetailView >> htmlView)
        |> App.mapError (Error >> hikeDetailView >> htmlView)
