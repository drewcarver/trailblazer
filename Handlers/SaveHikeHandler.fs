namespace HikePlanner.Handlers

open Giraffe
open HikePlanner.Core
open HikePlanner.Core.Utils
open HikePlanner.Infrastructure
open HikePlanner.Repositories.HikeRepoHikes
open HikePlanner.Views.Hikes.CreateHike

module SaveHikeHandler =
    let private parseCampPoints (form: Common.SaveHikeForm) =
        form.CampPoints
        |> List.map (fun campPoint ->
            campPoint
            |> tryParseInt
            |> Result.mapError (fun _ -> FormValidationError ("Invalid camp point: " + campPoint))
            |> App.ofResult)

    let saveHikeHandler : TrailblazerEndpoint<_> =
        app {
            let! ctx = App.asks (fun env -> env.Context)

            let! form: Common.SaveHikeForm = getFormHelper ctx
            let! validatedForm = Common.validateSaveHikeForm form |> App.ofResult
            let! userProfile = Common.getUserProfile |> App.ofAppResult

            let! campPoints = parseCampPoints form

            let! id = saveHike userProfile.Email validatedForm.HikeName validatedForm.StartDate campPoints

            return!
                App.succeed (
                    setHttpHeader "x-hike-id" id >=>
                    setHttpHeader "HX-Location" "/hikes" >=>
                    setStatusCode 204
                )
        }
        |> App.mapError (fun error -> createHikeView None [] (Error error) Create None |> htmlView)

    let updateHikeHandler hikeId : TrailblazerEndpoint<_> =
        app {
            let! ctx = App.asks (fun env -> env.Context)

            let! form: Common.SaveHikeForm = getFormHelper ctx
            let! validatedForm = Common.validateSaveHikeForm form |> App.ofResult
            let! userProfile = Common.getUserProfile |> App.ofAppResult

            let! campPoints = parseCampPoints form

            let! id = updateHike hikeId userProfile.Email validatedForm.HikeName validatedForm.StartDate campPoints

            return!
                App.succeed (
                    setHttpHeader "x-hike-id" id >=>
                    setHttpHeader "HX-Location" "/hikes" >=>
                    setStatusCode 204
                )
        }
        |> App.mapError (fun error -> createHikeView None [] (Error error) (Edit hikeId) None |> htmlView)