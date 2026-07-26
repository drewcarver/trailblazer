namespace HikePlanner.Handlers

open Giraffe
open HikePlanner.Core
open HikePlanner.Infrastructure

module HomeHandler =
    open HikePlanner.Views.Home
    let homeHandler : TrailblazerEndpoint =
        app {
            let! userProfile = Common.getUserProfile

            return Some userProfile 
                |> indexView
                |> htmlView
        } 
        |> App.mapError (function 
            | _ -> indexView None |> htmlView
        )