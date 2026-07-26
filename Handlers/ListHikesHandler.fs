namespace HikePlanner.Handlers

open Giraffe
open HikePlanner.Core
open HikePlanner.Infrastructure
open HikePlanner.Repositories.HikeRepoHikes
open HikePlanner.Views.Hikes.ListHikes

module ListHikesHandler =
    let listHikesHandler: TrailblazerEndpoint =
        app {
            let! userProfile = Common.getUserProfile 
            let! hikes = getHikes userProfile.Email

            return htmlView (listHikes (Some userProfile) (Ok hikes))
        }
        |> App.mapError (Error >> listHikes None >> htmlView)