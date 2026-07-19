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

    let private containsIgnoreCase (value: string) (query: string) =
        value.Contains(query, StringComparison.OrdinalIgnoreCase)

    let private friendMatchesQuery (query: string) (friend: Friend) =
        containsIgnoreCase friend.Email query
        || (not (String.IsNullOrWhiteSpace(friend.Name)) && containsIgnoreCase friend.Name query)

    [<CLIMutable>]
    type SaveHikeForm = {
        HikeName: string
        StartDate: DateTime
        CampPoints: string list
    }

    let private validateSaveHikeForm (form: SaveHikeForm) =
        let normalizedName = form.HikeName.Trim()

        if String.IsNullOrWhiteSpace normalizedName then
            Error (FormValidationError "HikeName is required.")
        elif List.isEmpty form.CampPoints then
            Error (FormValidationError "At least one camp point is required.")
        else
            Ok { form with HikeName = normalizedName }

    let getUserProfile: App<_, _, Result<UserProfile, TrailblazerError>> =
        App.asks (fun env ->
            let findClaim claimType =
                env.Context.User.FindFirst(claimType: string) |> Option.ofObj |> Option.map (fun c -> c.Value)

            let name = findClaim ClaimTypes.Name
            let email = findClaim ClaimTypes.Email
            let picture = findClaim "urn:google:picture"

            match name, email, picture with
            | Some name, Some email, picture -> Ok { Name = name; Picture = picture; Email = email }
            | _                              -> Error (FormValidationError "User profile is missing required claims.")
        )


    let accountHandler : TrailblazerEndpoint<_> =
        app {
            let! userProfile = getUserProfile |> App.ofAppResult

            let! existingFriends =
                getUser userProfile.Email
                |> App.map (fun user -> user.Friends)
                |> App.mapResult (function
                    | Ok friends -> Ok friends
                    | Error (NotFound _) -> Ok []
                    | Error e -> Error e)

            do! saveUser { Email = userProfile.Email; Picture = userProfile.Picture; Name = userProfile.Name; Friends = existingFriends }

            return redirectTo false "/plan"
        }
        |> App.mapError (fun _ -> setStatusCode 500 >=> text "Unable to complete sign in.")

    let listPlansHandler: TrailblazerEndpoint<_> =
        app {
            let! hikes = getHikes 
            and! userProfile = getUserProfile |> App.ofAppResult

            return htmlView (listPlans (Some userProfile) (Ok hikes))
        } 
        |> App.mapError (Error >> listPlans None >> htmlView)

    let planHandler: TrailblazerEndpoint<_> =
        app {
            let! trailPointsOfInterest = getTrailPointsOfInterest "AppalachianTrail" 
            and! userProfile = getUserProfile |> App.ofAppResult

            return htmlView (Plan.planView (Some userProfile) (Ok trailPointsOfInterest))
        }
        |> App.mapError (Error >> Plan.planView None >> htmlView) 

    let saveHikePlan : TrailblazerEndpoint<_> =
        app {
            let! ctx = App.asks(fun env -> env.Context)

            let! form: SaveHikeForm = getFormHelper ctx 
            let! validatedForm = validateSaveHikeForm form |> App.ofResult

            let! campPoints = form.CampPoints |> List.map (
                fun cp ->
                    cp
                    |> tryParseInt
                    |> Result.mapError (fun _ -> FormValidationError ("Invalid camp point: " + cp))
                    |> App.ofResult)

            let! id =  saveHike validatedForm.HikeName validatedForm.StartDate campPoints

            return! App.succeed (
                setHttpHeader "x-hike-id" id >=>
                setHttpHeader "HX-Location" "/plan" >=> setStatusCode 204
            )
        } 
        |> App.mapError (Error >> Plan.planView None >> htmlView) 

    let viewHikeHandler hikeId : TrailblazerEndpoint<_> =
        app {
            let! hike = getHikeById hikeId
            and! userProfile = getUserProfile |> App.ofAppResult

            return htmlView (hikeDetailView (Some userProfile) (Ok hike))
        }
        |> App.mapError (Error >> hikeDetailView None >> htmlView)

    let listHikersHandler : TrailblazerEndpoint<_> =
        app {
            let! ctx = App.asks(fun env -> env.Context)
            let! userProfile = getUserProfile |> App.ofAppResult
            let! user = getUser userProfile.Email

            let searchTerm =
                match ctx.TryGetQueryStringValue "friendSearch" with
                | Some term -> term.Trim()
                | None -> String.Empty

            let matchedFriends =
                if String.IsNullOrWhiteSpace searchTerm then
                    []
                else
                    user.Friends |> List.filter (friendMatchesQuery searchTerm)

            return listHikersResultsView searchTerm (Ok matchedFriends) |> htmlView
        }
        |> App.mapError (Error >> listHikersResultsView String.Empty >> htmlView)