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
    open HikePlanner.Views.Plan.ListHikersResults
    [<CLIMutable>]
    type SaveHikeForm = {
        HikeName: string
        StartDate: DateTime
        CampPoints: string list
    }

    let getUserProfile =
        App.asks (fun env ->
            let findClaim claimType =
                env.Context.User.FindFirst(claimType: string) |> Option.ofObj |> Option.map (fun c -> c.Value)
            findClaim ClaimTypes.Name
            |> Option.map (fun name -> { Name = name; Picture = findClaim "urn:google:picture" }))
        |> App.bind (App.ofOption (FormValidationError "User profile not found"))


    let accountHandler : TrailblazerEndpoint<_> =
        app {
            let! userProfile = getUserProfile
            let! _ = saveUser { Email = userProfile.Name; Picture = userProfile.Picture; Name = userProfile.Name; Friends = [] }

            return redirectTo false "/plan"
        } |> App.mapError (sprintf "%A" >> text)

    let listPlansHandler: TrailblazerEndpoint<_> =
        app {
            let! hikes = getHikes 
            and! userProfile = getUserProfile

            return htmlView (listPlans (Some userProfile) (Ok hikes))
        } 
        |> App.mapError (Error >> listPlans None >> htmlView)

    let planHandler: TrailblazerEndpoint<_> =
        app {
            let! trailPointsOfInterest = getTrailPointsOfInterest "AppalachianTrail" 
            and! userProfile = getUserProfile

            return htmlView (Plan.planView (Some userProfile) (Ok trailPointsOfInterest))
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
        app {
            let! hike = getHikeById hikeId
            and! userProfile = getUserProfile

            return htmlView (hikeDetailView (Some userProfile) (Ok hike))
        }
        |> App.mapError (fun err -> Error err |> hikeDetailView None |> htmlView)

    let listHikersHandler : TrailblazerEndpoint<_> =
        app {
            let! userProfile = getUserProfile
            let! user = getUser userProfile.Name

            return listHikersResultsView (Some userProfile) (Ok user) |> htmlView
        }
        |> App.mapError (fun err -> Error err |> listHikersResultsView None |> htmlView)