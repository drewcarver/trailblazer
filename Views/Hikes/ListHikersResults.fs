module HikePlanner.Views.Hikes.ListHikersResults

open System
open Giraffe.ViewEngine
open HikePlanner.Core

let private isLikelyEmail (value: string) =
    let trimmed = value.Trim()
    trimmed.Contains("@") && trimmed.Contains(".")

let private friendOption (friend: Friend) =
    let friendName = if String.IsNullOrWhiteSpace(friend.Name) then friend.Email else friend.Name
    option [ _value friend.Email ] [ str (sprintf "%s (%s)" friendName friend.Email) ]

let listHikersResultsView (searchTerm: string) (hikersResult: Result<Friend list, TrailblazerError>) =
    let normalizedTerm = searchTerm.Trim()

    let matchedFriends =
        match hikersResult with
        | Ok friends -> friends
        | Error _ -> []

    let showInviteLink = isLikelyEmail normalizedTerm && matchedFriends.IsEmpty

    match hikersResult with
    | Ok friends ->
        div [] [
        datalist [ _id "friend-search-list" ] (matchedFriends |> List.map friendOption)
        if showInviteLink then
            div [ _class "mt-2" ] [
                a [
                    _href (sprintf "mailto:%s?subject=Join%%20me%%20on%%20Trailblazer&body=Join%%20Trailblazer%%20so%%20we%%20can%%20plan%%20our%%20hike%%20together." normalizedTerm)
                    _class "text-sm underline text-blue-700 hover:text-blue-900"
                ] [ str (sprintf "Invite Friend: %s" normalizedTerm) ]
            ]
        ]
    | Error _ ->
        div [] [
            p [ _class "text-sm text-red-600" ] [ str "An error occurred while searching for friends." ]
        ]