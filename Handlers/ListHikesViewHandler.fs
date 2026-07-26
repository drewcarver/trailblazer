namespace HikePlanner.Handlers

open Giraffe
open HikePlanner.Core
open HikePlanner.Infrastructure
open HikePlanner.Views.Hikes.ListHikesView

module ListHikesViewHandler =
    let listHikesViewHandler: TrailblazerEndpoint =
        app {
            let! userProfile = Common.getUserProfile 

            return Some userProfile 
                |> listHikesView
                |> htmlView
        } |> App.mapError (function 
            | _ -> listHikesView None 
                |> htmlView
        )