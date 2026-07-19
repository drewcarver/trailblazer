namespace HikePlanner.Handlers

open Giraffe
open HikePlanner.Core
open HikePlanner.Infrastructure
open HikePlanner.Repositories.HikeRepo
open HikePlanner.Views.Plan

module CreatePlanHandler =
    let planHandler: TrailblazerEndpoint<_> =
        app {
            let! trailPointsOfInterest = getTrailPointsOfInterest "AppalachianTrail"
            and! userProfile = Common.getUserProfile |> App.ofAppResult
            let! user = getUser userProfile.Email

            return htmlView (Plan.planView (Some userProfile) user.Friends (Ok trailPointsOfInterest))
        }
        |> App.mapError (Error >> Plan.planView None [] >> htmlView)