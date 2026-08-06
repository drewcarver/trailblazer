namespace HikePlanner.Handlers

open System
open System.Net.Http
open System.Security.Claims
open HikePlanner.Core
open HikePlanner.Infrastructure
open HikePlanner.Repositories.HikeRepoUsers

module Common =
    let private avatarHttpClient = new HttpClient()

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

    let private tryGetStoredUser email =
        getUser email
        |> App.map Some
        |> App.mapResult (function
            | Ok user -> Ok user
            | Error (NotFound _) -> Ok None
            | Error error -> Error error)

    let tryGetAvatarDataUri pictureUrl =
        app {
            if String.IsNullOrWhiteSpace pictureUrl then
                return None
            else
                let! avatarDataUri =
                    App.catchAsync
                        (fun _ -> DatabaseError "Failed to cache avatar image.")
                        (fun () ->
                            task {
                                use! response = avatarHttpClient.GetAsync(pictureUrl, HttpCompletionOption.ResponseHeadersRead)

                                if not response.IsSuccessStatusCode then
                                    return None
                                else
                                    let! bytes = response.Content.ReadAsByteArrayAsync()
                                    let mediaType =
                                        response.Content.Headers.ContentType
                                        |> Option.ofObj
                                        |> Option.map (fun contentType -> contentType.MediaType)
                                        |> Option.filter (String.IsNullOrWhiteSpace >> not)
                                        |> Option.defaultValue "image/jpeg"

                                    return Some (sprintf "data:%s;base64,%s" mediaType (Convert.ToBase64String bytes))
                            })
                return avatarDataUri
        }

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
            let! storedUser = tryGetStoredUser email
            let picture =
                storedUser
                |> Option.bind (fun existingUser -> existingUser.Picture)
                |> Option.orElseWith (fun () -> findClaim "urn:google:picture")

            return { Id = email; Name = name; Picture = picture; Email = email }
        }