namespace HikePlanner.Handlers

open Giraffe
open HikePlanner.Core
open HikePlanner.Infrastructure
open HikePlanner.Repositories.HikeRepo
open HikePlanner.Views.Plan.ListPlans

module ListPlansHandler =
    let listPlansHandler: TrailblazerEndpoint<_> =
        app {
            let! hikes = getHikes
            and! userProfile = Common.getUserProfile |> App.ofAppResult

            return htmlView (listPlans (Some userProfile) (Ok hikes))
        }
        |> App.mapError (Error >> listPlans None >> htmlView)