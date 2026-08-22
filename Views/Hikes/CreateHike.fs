module HikePlanner.Views.Hikes.CreateHike

open System;
open Giraffe.ViewEngine
open HikePlanner.Views.Components.Select
open HikePlanner.Views.MasterLayout
open HikePlanner.Repositories.HikeRepoTypes
open HikePlanner.Core

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

let private inputGroup id name labelText inputType value placeholder attributes =
    label [ _for id; _class "input-group" ] [
        span [ _class "input-label" ] [ str labelText ]
        input ([
            _id id
            _name name
            _type inputType
            _value value
            _placeholder placeholder
            _class "input-control"
        ] @ attributes)
    ]

let private campPointSelect day (options: SelectOption list) (selectedPoint: string option) =
    let id = sprintf "camp-point-select-day-%d" day

    div [ _class "day-selector" ] [
        div [ _class "day-label" ] [ str (sprintf "Day %d" day) ]
        label [ _for id; _class "input-group" ] [
            span [ _class "input-label" ] [ str "End Point" ]
            select [
                _id id
                _name "campPoints"
                _class "select-control"
                attr "required" "required"
                attr "_" "install SelectPoint"
                attr "data-day" (string day)
            ] [
                option [
                    _value ""
                    attr "disabled" "disabled"
                    if selectedPoint.IsNone then attr "selected" "selected"
                ] [ str "Select end location" ]
                for selectOption in options do
                    option [
                        _value selectOption.Value
                        if selectedPoint = Some selectOption.Value then attr "selected" "selected"
                        yield! selectOption.Attributes |> Seq.map (fun (key, value) -> attr key value)
                    ] [ str selectOption.Label ]
            ]
        ]
        i [ _class "route-arrow fa-solid fa-arrow-right"; attr "aria-hidden" "true" ] []
        button [ _type "button"; _class "more-options"; attr "aria-label" "More options" ] [
            i [ _class "fa-solid fa-ellipsis-vertical"; attr "aria-hidden" "true" ] []
        ]
    ]

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

        form [
            _class "l-entry-page"
            attr "_" "install InitForm"
            attr "action" (submitPath mode)
            attr "method" "post"
            attr "hx-post" (submitPath mode)
            attr "hx-swap" "outerHTML"
        ] [
            section [ _class "l-new-hike-section"; attr "aria-labelledby" "page-title" ] [
                a [ _class "breadcrumb"; _href "/hikes" ] [
                    i [ _class "fa-solid fa-chevron-left"; attr "aria-hidden" "true" ] []
                    str " Back to My Hikes"
                ]
                header [] [
                    h1 [ _id "page-title" ] [ str (formHeader mode) ]
                    p [ _class "page-description" ] [ str "Plan your route, add camp locations, and invite friends." ]
                ]
                section [ _class "card"; attr "aria-labelledby" "details-title" ] [
                    h2 [ _id "details-title" ] [ str "Hike Details" ]
                    div [ _class "l-input-row" ] [
                        inputGroup "hike-name" "hikeName" "Hike Name" "text" hikeNameValue "e.g., High Sierra Adventure" [ attr "required" "required" ]
                        label [ _for "start-date"; _class "input-group" ] [
                            span [ _class "input-label" ] [ str "Start Date" ]
                            span [ _class "input-with-icon" ] [
                                input [
                                    _id "start-date"
                                    _name "startDate"
                                    _type "date"
                                    _value ((startDateValue |> Option.defaultValue DateTime.Now).ToString("yyyy-MM-dd"))
                                    _min (match mode with | Create -> DateTime.Now.ToString("yyyy-MM-dd") | Edit _ -> "")
                                    _class "input-control"
                                    attr "required" "required"
                                ]
                                i [ _class "fa-regular fa-calendar"; attr "aria-hidden" "true" ] []
                            ]
                        ]
                        label [ _for "friend-search"; _class "input-group" ] [
                            span [ _class "input-label" ] [ str "Invite Friends" ]
                            input [ _type "hidden"; _name "invitees"; _value "" ]
                            span [ _class "input-with-icon" ] [
                                input [
                                    _id "friend-search"
                                    _type "text"
                                    _class "input-control"
                                    attr "list" "friend-search-list"
                                    attr "data-field-name" "invitees"
                                    attr "data-list-id" "friend-search-list"
                                    attr "data-badges-id" "friend-search-badges"
                                    attr "_" "install Autosuggest"
                                    _placeholder "Search friends..."
                                    attr "autocomplete" "off"
                                ]
                                i [ _class "fa-regular fa-user"; attr "aria-hidden" "true" ] []
                            ]
                            div [ _id "friend-search-badges"; _class "autosuggest__badges" ] []
                            datalist [ _id "friend-search-list"; _class "autosuggest__list" ] [
                                for value, label in friendOptions do
                                    option [ _value value ] [ str label ]
                            ]
                        ]
                    ]
                ]
                section [ _class "card camp-locations-section"; attr "aria-labelledby" "camp-title" ] [
                    header [] [
                        h2 [ _id "camp-title" ] [ str "Camp Locations (Days)" ]
                        p [] [ str "Add each day of your hike by selecting an end point." ]
                    ]
                    for day, selectedPoint in campPointValuesByDay |> List.indexed |> List.map (fun (index, value) -> index + 1, value) do
                        campPointSelect day pointsOfInterestOptions selectedPoint
                    button ([
                        _id "add-point"
                        _type "button"
                        _class "btn btn-secondary add-day-btn"
                        attr "_" "install AddPoint"
                    ] @ if addPointDisabled then [ attr "disabled" "disabled" ] else []) [
                        i [ _class "fa-solid fa-plus"; attr "aria-hidden" "true" ] []
                        str " Add Day"
                    ]
                ]
                div [ _class "form-actions" ] [
                    button [ _id "submit-plan"; _type "submit"; _class "btn btn-primary" ] [ str "Save Hike" ]
                    a [ _href "/hikes"; _class "btn btn-secondary" ] [ str "Cancel" ]
                ]
            ]
            aside [ _class "card card-no-padding route-preview"; attr "aria-labelledby" "route-preview-title" ] [
                header [ _class "map-header" ] [
                    div [] [
                        h2 [ _id "route-preview-title" ] [ str "Route Preview" ]
                        p [] [ str "Review your planned route and daily segments." ]
                    ]
                ]
                div [ _id "map"; _class "map"; attr "aria-label" "Map showing the planned hike route" ] []
                match trailPointsOfInterest with
                | Ok _ -> emptyText
                | Error error -> span [ _class "form-panel__error" ] [ str (error.ToString()) ]
            ]
        ]
        |> XmlNodeBody |> withMasterLayout userName
