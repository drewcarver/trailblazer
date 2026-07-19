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

    let getUserProfile<'env> : App<EnvironmentWithContext<'env>, TrailblazerError, Result<UserProfile, TrailblazerError>> =
        App.asks (fun env ->
            let findClaim claimType =
                env.Context.User.FindFirst(claimType: string)
                |> Option.ofObj
                |> Option.map (fun claim -> claim.Value)

            let name = findClaim ClaimTypes.Name
            let email = findClaim ClaimTypes.Email
            let picture = findClaim "urn:google:picture"

            match name, email, picture with
            | Some profileName, Some profileEmail, profilePicture ->
                Ok { Name = profileName; Picture = profilePicture; Email = profileEmail }
            | _ ->
                Error (FormValidationError "User profile is missing required claims.")
        )