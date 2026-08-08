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

        div [ _class "page-section create-hike" ] [
            h1 [ _class "form-panel__title" ] [ str (formHeader mode) ]
            div [ _class "create-hike__layout" ] [
                form [ _class "form-panel"; 
                        attr "_" (sprintf "install InitForm");
                        attr "action" (submitPath mode);
                        attr "method" "post";
                        attr "hx-post" (submitPath mode); 
                        attr "hx-swap" "outerHTML" ] [
                            div [ _class "field" ] [
                                label [ _for "hike-name"; _class "field__label" ] [ str "Hike Name" ]
                                input [ 
                                    _type "text"; 
                                    _id "hike-name"; 
                                    _name "hikeName"; 
                                    attr "required" "required"
                                    _value hikeNameValue
                                    _class "field__control" 
                                ]
                                span [ _class "field__message" ] [ 
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
                            div [ _class "button-group" ] [
                                trailblazerButton (Some "add-point") "Add Point" "Add Point" "button" ([ "_", "install AddPoint" ] @ if addPointDisabled then [ "disabled", "true" ] else [])
                                trailblazerButton (Some "remove-point") "Remove Point" "Remove Point" "button" removePointButtonAttrs
                                trailblazerButton (Some "submit-plan") "Submit Plan" "Submit Plan" "submit" []
                            ]
                ]
                div [ _class "create-hike__sidebar" ] [
                    div [ _id "map"; _class "map-pane" ] []
                    match trailPointsOfInterest with
                        | Ok _ -> emptyText
                        | Error e -> span [ _class "form-panel__error" ] [ 
                            match e with
                                | DatabaseError error -> str (e.ToString ())
                                | FormValidationError e -> str e
                                | NotFound e -> str e
                        ]
                ]
            ]
        ]
        |> XmlNodeBody |> withMasterLayout userName
