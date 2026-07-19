namespace HikePlanner.Handlers

open Giraffe
open HikePlanner.Core
open HikePlanner.Core.Utils
open HikePlanner.Infrastructure
open HikePlanner.Repositories.HikeRepo
open HikePlanner.Views.Hikes.CreateHike

module SaveHikePlanHandler =
    let saveHikePlan : TrailblazerEndpoint<_> =
        app {
            let! ctx = App.asks (fun env -> env.Context)

            let! form: Common.SaveHikeForm = getFormHelper ctx
            let! validatedForm = Common.validateSaveHikeForm form |> App.ofResult

            let! campPoints =
                form.CampPoints
                |> List.map (fun campPoint ->
                    campPoint
                    |> tryParseInt
                    |> Result.mapError (fun _ -> FormValidationError ("Invalid camp point: " + campPoint))
                    |> App.ofResult)

            let! id = saveHike validatedForm.HikeName validatedForm.StartDate campPoints

            return!
                App.succeed (
                    setHttpHeader "x-hike-id" id >=>
                    setHttpHeader "HX-Location" "/hikes" >=>
                    setStatusCode 204
                )
        }
        |> App.mapError (Error >> createHikeView None [] >> htmlView)