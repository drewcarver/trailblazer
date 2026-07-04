namespace HikePlanner.Handlers

open HikePlanner.App
open HikePlanner.Views.ListPlans
open HikePlanner.Repositories.HikeRepo
open Giraffe
open HikePlanner.Utilities.Utilities

module Handlers = 
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

    let listPlansHandler connectionString = (fun next ctx -> 
        task {
            let! result = App.run connectionString getSavedHikes

            return! htmlView (listPlans result) next ctx
        })