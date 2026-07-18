namespace HikePlanner.Handlers

open HikePlanner.Infrastructure
open HikePlanner.Repositories.HikeRepo
open HikePlanner.Core
open HikePlanner.Views.Plan
open HikePlanner.Views.Plan.ListPlans
open HikePlanner.Views.Plan.HikeDetail
open System
open System.Security.Claims
open Giraffe
open Utils

module Handlers = 
    [<CLIMutable>]
    type SaveHikeForm = {
        HikeName: string
        StartDate: DateTime
        CampPoints: string list
    }

    let getUserName =
        App.asks (fun env ->
            env.Context.User.FindFirst(ClaimTypes.Name) |> Option.ofObj |> Option.map (fun c -> c.Value))

    let listPlansHandler: TrailblazerEndpoint<_> =
        app {
            let! hikes = getHikes 
            and! userName = getUserName

            return htmlView (listPlans userName (Ok hikes))
        } 
        |> App.mapError (Error >> listPlans None >> htmlView)

    let planHandler: TrailblazerEndpoint<_> =
        app {
            let! trailPointsOfInterest = getTrailPointsOfInterest "AppalachianTrail" 
            and! userName = getUserName

            return htmlView (Plan.planView userName (Ok trailPointsOfInterest))
        }
        |> App.mapError (Error >> Plan.planView None >> htmlView) 

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
        |> App.mapError (fun err -> Error err |> Plan.planView None |> htmlView) 

    let viewHikeHandler hikeId : TrailblazerEndpoint<_> =
       App.zip getUserName (getHikeById hikeId)
        |> App.map (fun (userName, hike) -> Ok hike |> hikeDetailView userName |> htmlView)
        |> App.mapError (fun err -> Error err |> hikeDetailView None |> htmlView)
