module HikePlanner.Views.Hikes.CreateHike

open System;
open Giraffe.ViewEngine
open HikePlanner.Views.Components.TextInput
open HikePlanner.Views.Components.DatePicker
open HikePlanner.Views.Components.Select
open HikePlanner.Views.MasterLayout
open HikePlanner.Repositories.HikeRepoTypes
open HikePlanner.Core
open HikePlanner.Views.Components.Button
open HikePlanner.Views.Components.Autosuggest

let createHikeView userName (friends: Friend list) (trailPointsOfInterest: Result<TrailPointOfInterest list, TrailblazerError>) =
        let toOptionLabel (poi: TrailPointOfInterest) = sprintf "%s - Mile %.2f" poi.Name poi.TrailMile
        let pointsOfInterestOptions = 
            trailPointsOfInterest 
            |> Result.defaultWith (fun _ -> List.empty)
            |> Seq.map (fun poi -> { Label = poi |> toOptionLabel; Value = string poi.Id; Attributes = [ ("data-mile", poi.TrailMile.ToString())] })

        let friendOptions =
            friends
            |> List.map (fun friend ->
                let friendName =
                    if String.IsNullOrWhiteSpace friend.Name then friend.Email else friend.Name

                friend.Email, sprintf "%s (%s)" friendName friend.Email)

        div [ _class "p-6 border border-black rounded-lg bg-white p-4 font-sans selection:bg-neutral-200 m-2"] [
            div [ _class "flex gap-4 flex-wrap" ] [
                h1 [ _class "text-2xl font-mono font-bold mb-4" ] [ str "Create New Hike" ]
                div [ _class "flex items-center gap-4" ] [
                    form [ _class "font-mono mx-auto transition-[width] duration-500 ease-in-out"; 
                        attr "_" "install ListenForSelectChange";
                        attr "hx-post" "/hikes"; attr "hx-swap" "outerHTML" ] [
                        textInput "hike-name" "hikeName" "Hike Name" true Required
                        datePicker "start-date" "startDate" "Start Date" (Some DateTime.Now) Required []
                        trailblazerAutosuggest "friend-search" "invitees" "Invite Friends" Optional friendOptions [
                            "placeholder", "Type friend name or email"
                            "autocomplete", "off"
                        ]
                        trailblazerSelect "camp-point-select-day-1" "campPoints" "Day 1" Required pointsOfInterestOptions [ "_", "install SelectPoint"; "data-day", "1" ]
                        trailblazerSelect "camp-point-select-day-2" "campPoints" "Day 2" Required pointsOfInterestOptions [ "_", "install SelectPoint"; "data-day", "2" ]
                        div [ _class "flex gap-2 mt-4" ] [
                            trailblazerButton (Some "add-point") "Add Point" "Add Point" "button" [ "_", "install AddPoint"; "disabled", "true" ]
                            trailblazerButton (Some "remove-point") "Remove Point" "Remove Point" "button" [ "_", "install RemovePoint" ]
                            trailblazerButton (Some "submit-plan") "Submit Plan" "Submit Plan" "submit" []
                        ]
                    ]
                    div [ _id "map"; _class "w-[60vw] min-w-[300px] h-[400px]"; attr "_" "on load call initializeMap(me)" ] []
                    match trailPointsOfInterest with
                        | Ok _ -> emptyText
                        | Error e -> span [] [ 
                            match e with
                                | DatabaseError error -> str "An error ocurred when retrieving points of interest." 
                                | FormValidationError e -> str e
                                | NotFound e -> str e
                        ]
                ]
            ]
        ]
        |> XmlNodeBody |> withMasterLayout userName
