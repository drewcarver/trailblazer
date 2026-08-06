namespace HikePlanner.Handlers

open Giraffe
open HikePlanner.Core
open HikePlanner.Infrastructure
open HikePlanner.Repositories.HikeRepoUsers

module AccountHandler =
    let accountHandler : TrailblazerEndpoint =
        app {
            let! userProfile = Common.getUserProfile 

            let! existingUser =
                getUser userProfile.Email
                |> App.map Some
                |> App.mapResult (function
                    | Ok user -> Ok user
                    | Error (NotFound _) -> Ok None
                    | Error error -> Error error)

            let! picture =
                match userProfile.Picture with
                | Some pictureUrl when pictureUrl.StartsWith("data:", System.StringComparison.OrdinalIgnoreCase) ->
                    App.succeed (Some pictureUrl)
                | Some pictureUrl ->
                    Common.tryGetAvatarDataUri pictureUrl
                    |> App.map (Option.orElseWith (fun () -> existingUser |> Option.bind (fun user -> user.Picture)))
                | None ->
                    App.succeed (existingUser |> Option.bind (fun user -> user.Picture))

            let existingFriends =
                existingUser
                |> Option.map (fun user -> user.Friends)
                |> Option.defaultValue []

            do!
                saveUser {
                    Email = userProfile.Email
                    Picture = picture
                    Name = userProfile.Name
                    Friends = existingFriends
                }

            return redirectTo false "/hikes"
        }
        |> App.mapError (fun e -> setStatusCode 500 >=> text (e.ToString ()))