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

type HikeFormMode =
    | Create
    | Edit of int64

let private formHeader mode =
    match mode with
    | Create -> "Create New Hike"
    | Edit _ -> "Edit Hike"

let private submitPath mode =
    match mode with
    | Create -> "/hikes"
    | Edit hikeId -> sprintf "/hikes/%d" hikeId

let createHikeView userName (friends: Friend list) (trailPointsOfInterest: Result<TrailPointOfInterest list, TrailblazerError>) mode (existingHike: SavedHike option) =
        let toOptionLabel (poi: TrailPointOfInterest) = sprintf "%s - Mile %.2f" poi.Name poi.TrailMile
        let pointsOfInterestOptions = 
            trailPointsOfInterest 
            |> Result.defaultWith (fun _ -> List.empty)
            |> Seq.map (fun poi -> { Label = poi |> toOptionLabel; Value = string poi.Id; Attributes = [ ("data-mile", poi.TrailMile.ToString())] })
            |> Seq.toList

        let existingCampPointIds =
            existingHike
            |> Option.map (fun hike -> hike.CampPoints |> List.map (fun point -> string point.Id))
            |> Option.defaultValue []

        let totalCampPointDays = max 2 existingCampPointIds.Length

        let campPointValuesByDay =
            [ 1 .. totalCampPointDays ]
            |> List.map (fun day -> existingCampPointIds |> List.tryItem (day - 1))

        let addPointDisabled =
            campPointValuesByDay
            |> List.exists Option.isNone

        let removePointButtonAttrs =
            [ "_", "install RemovePoint" ]
            @ if totalCampPointDays = 2 then [ "disabled", "true" ] else []

        let hikeNameValue = existingHike |> Option.map (fun hike -> hike.Trail) |> Option.defaultValue ""
        let startDateValue = existingHike |> Option.map (fun hike -> hike.StartDate)

        let friendOptions =
            friends
            |> List.map (fun friend ->
                let friendName =
                    if String.IsNullOrWhiteSpace friend.Name then friend.Email else friend.Name

                friend.Email, sprintf "%s (%s)" friendName friend.Email)

        div [ _class "p-6 border border-black rounded-lg bg-white p-4 font-sans selection:bg-neutral-200 m-2"] [
            div [ _class "flex gap-4 flex-wrap" ] [
                h1 [ _class "text-2xl font-mono font-bold mb-4" ] [ str (formHeader mode) ]
                div [ _class "flex items-center gap-4" ] [
                    form [ _class "font-mono mx-auto transition-[width] duration-500 ease-in-out"; 
                        attr "_" (sprintf "install ListenForSelectChange on load set $campCounter to %d" totalCampPointDays);
                        attr "hx-post" (submitPath mode); attr "hx-swap" "outerHTML" ] [
                        div [ _class "mb-4" ] [
                            label [ _for "hike-name"; _class "block text-sm font-medium mb-1" ] [ str "Hike Name" ]
                            input [ 
                                _type "text"; 
                                _id "hike-name"; 
                                _name "hikeName"; 
                                attr "required" "required"
                                _value hikeNameValue
                                _class "peer w-full border border-gray-300 rounded-lg px-3 py-2 focus:outline-none focus:ring-2 focus:ring-[#4A7043]" 
                            ]
                            span [ _class "invisible opacity-0 peer-user-invalid:visible peer-user-invalid:opacity-100 transition-all duration-300 ease-in-out block text-xs text-red-500 mt-1" ] [ 
                                str "Please enter a value." 
                            ]
                        ]
                        datePicker
                            "start-date"
                            "startDate"
                            "Start Date"
                            (match mode with | Create -> Some DateTime.Now | Edit _ -> None)
                            Required
                            [ "value", (startDateValue |> Option.defaultValue DateTime.Now).ToString("yyyy-MM-dd") ]
                        trailblazerAutosuggest "friend-search" "invitees" "Invite Friends" Optional friendOptions [
                            "placeholder", "Type friend name or email"
                            "autocomplete", "off"
                        ]
                        for day, selectedPoint in campPointValuesByDay |> List.indexed |> List.map (fun (index, value) -> index + 1, value) do
                            trailblazerSelectWithSelected
                                (sprintf "camp-point-select-day-%d" day)
                                "campPoints"
                                (sprintf "Day %d" day)
                                Required
                                pointsOfInterestOptions
                                selectedPoint
                                [ "_", "install SelectPoint"; "data-day", string day ]
                        div [ _class "flex gap-2 mt-4" ] [
                            trailblazerButton (Some "add-point") "Add Point" "Add Point" "button" ([ "_", "install AddPoint" ] @ if addPointDisabled then [ "disabled", "true" ] else [])
                            trailblazerButton (Some "remove-point") "Remove Point" "Remove Point" "button" removePointButtonAttrs
                            trailblazerButton (Some "submit-plan") "Submit Plan" "Submit Plan" "submit" []
                        ]
                    ]
                    div [ _id "map"; _class "w-[60vw] min-w-[300px] h-[400px]"; attr "_" "on load call initializeMap(me)" ] []
                    match trailPointsOfInterest with
                        | Ok _ -> emptyText
                        | Error e -> span [] [ 
                            match e with
                                | DatabaseError error -> str (e.ToString ())
                                | FormValidationError e -> str e
                                | NotFound e -> str e
                        ]
                ]
            ]
        ]
        |> XmlNodeBody |> withMasterLayout userName
