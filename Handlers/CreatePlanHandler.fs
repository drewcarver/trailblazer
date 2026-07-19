namespace HikePlanner.Handlers

open Giraffe
open HikePlanner.Core
open HikePlanner.Infrastructure
open HikePlanner.Repositories.HikeRepoUsers
open HikePlanner.Repositories.HikeRepoTrailPoints
open HikePlanner.Views.Hikes.CreateHike

module CreatePlanHandler =
    let planHandler: TrailblazerEndpoint<_> =
        app {
            let! trailPointsOfInterest = getTrailPointsOfInterest "AppalachianTrail"
            and! userProfile = Common.getUserProfile |> App.ofAppResult
            let! user = getUser userProfile.Email

            return htmlView (createHikeView (Some userProfile) user.Friends (Ok trailPointsOfInterest))
        }
        |> App.mapError (Error >> createHikeView None [] >> htmlView)