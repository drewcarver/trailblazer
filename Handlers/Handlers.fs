namespace HikePlanner.Handlers

open HikePlanner.Infrastructure
open HikePlanner.Repositories.HikeRepo
open HikePlanner.Core
open Giraffe
open HikePlanner.Core.Utils
open HikePlanner.Views.Plan
open HikePlanner.Views.Plan.ListPlans

module Handlers = 
    open Giraffe.ViewEngine
    [<CLIMutable>]
    type SaveHikeForm = {
        TrailName: string
        StartDate: string
        EndDate: string
    }

    let saveHikePlan: App<(ConnectionString * Microsoft.AspNetCore.Http.HttpContext), string, SaveHikeForm> =
        app {
            let! _, ctx = App.ask
            let! form = ctx.TryBindFormAsync<SaveHikeForm>() 

            let! startDate = tryParseDate form.StartDate 
            let! endDate = tryParseDate form.EndDate 

            let! _ = 
                saveHike form.TrailName startDate endDate
                |> App.mapError (fun err ->
                    match err with
                    | DatabaseError msg -> sprintf "Database error: %s" msg
                    | NotFound msg -> sprintf "Not found: %s" msg
                )
            
            return form
        } 

    let listPlansHandler connectionString : HttpHandler = (fun next ctx -> 
        task {
            let! result = App.run connectionString getSavedHikes

            return! htmlView (ListPlans.listPlans result) next ctx
        })


    // planHandler is not currently used - reserved for future expansion
    let planHandler : App<ConnectionString, HttpHandler, HttpHandler>=
        app {
            let! plans = getTrailPointsOfInterest "AppalachianTrail"

            return htmlView (Plan.planView plans)
        } |> App.mapError (fun err ->
            match err with
            | DatabaseError msg -> htmlView (div [] [ str (sprintf "Database error: %s" msg) ])
            | NotFound msg -> htmlView (div [] [ str (sprintf "Not found: %s" msg) ])
        )