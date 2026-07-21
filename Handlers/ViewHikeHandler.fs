namespace HikePlanner.Handlers

open Giraffe
open HikePlanner.Core
open HikePlanner.Infrastructure
open HikePlanner.Repositories.HikeRepoHikes
open HikePlanner.Repositories.HikeRepoTrailPoints
open HikePlanner.Views.Hikes.CreateHike

module ViewHikeHandler =
    let viewHikeHandler hikeId : TrailblazerEndpoint<_> =
        app {
            let! hike = getHikeById hikeId
            let! trailPointsOfInterest = getTrailPointsOfInterest "AppalachianTrail"
            and! userProfile = Common.getUserProfile |> App.ofAppResult

            return htmlView (createHikeView (Some userProfile) [] (Ok trailPointsOfInterest) (Edit hikeId) (Some hike))
        }
        |> App.mapError (fun error -> createHikeView None [] (Error error) (Edit hikeId) None |> htmlView)