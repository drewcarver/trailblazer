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
        CampPoints: string list
    }

    let listPlansHandler: TrailblazerEndpoint<_> =
        getHikes 
        |> App.map (Ok >> listPlans >> htmlView)
        |> App.mapError (Error >> listPlans >> htmlView)

    let planHandler: TrailblazerEndpoint<_> =
        getTrailPointsOfInterest "AppalachianTrail" 
        |> App.map (Ok >> Plan.planView >> htmlView)
        |> App.mapError (Error >> Plan.planView >> htmlView) 

    let saveHikePlan : TrailblazerEndpoint<_> =
        app {
            let! ctx = App.asks(fun env -> env.Context)

            let! form: SaveHikeForm = getFormHelper ctx 

            let! campPoints = form.CampPoints |> List.map (
                tryParseInt >> Result.mapError (always FormValidationError "Not an int") >> App.ofResult)

            let! id =  saveHike form.HikeName form.StartDate campPoints

            return! App.succeed (
                setHttpHeader "x-hike-id" id >=>
                setHttpHeader "HX-Location" "/plan" >=> setStatusCode 204
            )
        } 
        |> App.mapError (Error >> Plan.planView >> htmlView) 

    let viewHikeHandler hikeId : TrailblazerEndpoint<_> =
       getHikeById hikeId
        |> App.map (Ok >> hikeDetailView >> htmlView)
        |> App.mapError (Error >> hikeDetailView >> htmlView)
