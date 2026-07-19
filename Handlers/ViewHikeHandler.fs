namespace HikePlanner.Handlers

open Giraffe
open HikePlanner.Core
open HikePlanner.Infrastructure
open HikePlanner.Repositories.HikeRepo
open HikePlanner.Views.Hikes.HikeDetail

module ViewHikeHandler =
    let viewHikeHandler hikeId : TrailblazerEndpoint<_> =
        app {
            let! hike = getHikeById hikeId
            and! userProfile = Common.getUserProfile |> App.ofAppResult

            return htmlView (hikeDetailView (Some userProfile) (Ok hike))
        }
        |> App.mapError (Error >> hikeDetailView None >> htmlView)