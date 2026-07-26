namespace HikePlanner.Handlers

open System
open System.Security.Claims
open HikePlanner.Core
open HikePlanner.Infrastructure

module Common =
    [<CLIMutable>]
    type SaveHikeForm = {
        HikeName: string
        StartDate: DateTime
        CampPoints: string list
        Invitees: string list
    }

    let validateSaveHikeForm (form: SaveHikeForm) =
        let normalizedName = form.HikeName.Trim()
        let normalizedInvitees =
            form.Invitees
            |> Option.ofObj
            |> Option.defaultValue []
            |> List.filter (fun invitee -> not (String.IsNullOrWhiteSpace invitee))

        if String.IsNullOrWhiteSpace normalizedName then
            Error (FormValidationError "HikeName is required.")
        elif List.isEmpty form.CampPoints then
            Error (FormValidationError "At least one camp point is required.")
        else
            Ok { form with HikeName = normalizedName; Invitees = normalizedInvitees }

    let getUserProfile =
        app {
            let! user = App.asks (fun env -> env.Context.User)
            let findClaim claimType =
                user.FindFirst(claimType: string)
                |> Option.ofObj
                |> Option.map (fun claim -> claim.Value)
            let userProfileNotFoundError = NotFound "User profile is missing required claims."

            let! name = findClaim ClaimTypes.Name |> App.ofOption userProfileNotFoundError
            let! email = findClaim ClaimTypes.Email |> App.ofOption userProfileNotFoundError
            let picture = findClaim "urn:google:picture" 

            return { Id = email; Name = name; Picture = picture; Email = email }
        }