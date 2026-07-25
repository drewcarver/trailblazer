namespace HikePlanner.Handlers

open Giraffe
open HikePlanner.Core
open HikePlanner.Infrastructure
open HikePlanner.Repositories.HikeRepoUsers

module AccountHandler =
    let accountHandler : TrailblazerEndpoint<_> =
        app {
            let! userProfile = Common.getUserProfile |> App.ofAppResult

            let! existingFriends =
                getUser userProfile.Email
                |> App.map (fun user -> user.Friends)
                |> App.mapResult (function
                    | Ok friends -> Ok friends
                    | Error (NotFound _) -> Ok []
                    | Error error -> Error error)

            do!
                saveUser {
                    Email = userProfile.Email
                    Picture = userProfile.Picture
                    Name = userProfile.Name
                    Friends = existingFriends
                }

            return redirectTo false "/hikes"
        }
        |> App.mapError (fun e -> setStatusCode 500 >=> text (e.ToString ()))