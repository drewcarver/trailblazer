namespace HikePlanner.Handlers

open Giraffe
open HikePlanner.Core
open HikePlanner.Infrastructure
open HikePlanner.Repositories.HikeRepoUsers
open HikePlanner.Repositories.HikeRepoTrailPoints
open HikePlanner.Views.Hikes.CreateHike

module CreateHikeHandler =
    let createHikeHandler: TrailblazerEndpoint =
        app {
            let! trailPointsOfInterest = getTrailPointsOfInterest "AppalachianTrail"
            and! userProfile = Common.getUserProfile 
            let! user = getUser userProfile.Email

            return htmlView (createHikeView (Some userProfile) user.Friends (Ok trailPointsOfInterest) Create None)
        }
        |> App.mapError (fun error -> createHikeView None [] (Error error) Create None |> htmlView)