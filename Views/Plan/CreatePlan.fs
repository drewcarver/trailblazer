module HikePlanner.Views.Plan.Plan

open System;
open Giraffe.ViewEngine
open HikePlanner.Views.Components.TextInput
open HikePlanner.Views.Components.DatePicker
open HikePlanner.Views.Components.Select
open HikePlanner.Views.MasterLayout
open HikePlanner.Repositories.HikeRepo
open HikePlanner.Core
open HikePlanner.Views.Components.Button

let planView (trailPointsOfInterest: Result<TrailPointOfInterest list, TrailblazerError>) =
        let toOptionLabel poi = sprintf "%s - Mile %.2f" poi.Name poi.TrailMile
        let pointsOfInterestOptions = 
            trailPointsOfInterest 
            |> Result.defaultWith (fun _ -> List.empty)
            |> Seq.map (fun poi -> { Label = poi |> toOptionLabel; Value = string poi.Id; Attributes = [ ("data-mile", poi.TrailMile.ToString())] })

        div [] [
            script [ _src "/js/trailmap.js"] []
            div [ _class "p-6 border border-black rounded-lg bg-white p-4 font-sans selection:bg-neutral-200 m-2"] [
                h1 [] [ str "Create New Hike" ]
                div [ _class "flex items-center gap-1" ] [
                    form [ _class "font-mono mx-auto mt-8 transition-[width] duration-500 ease-in-out"; attr "hx-post" "/plan"; attr "hx-swap" "outerHTML" ] [
                        textInput "hike-name" "hikeName" "Hike Name" true Required
                        datePicker "start-date" "startDate" "Start Date" (Some DateTime.Now) Required []
                        trailblazerSelect "camp-point-select-day-1" "campPoints" "Day 1" Required pointsOfInterestOptions [ "_", "install SelectPoint"; "data-day", "1" ]
                        trailblazerSelect "camp-point-select-day-2" "campPoints" "Day 2" Required pointsOfInterestOptions [ "_", "install SelectPoint"; "data-day", "2" ]
                        trailblazerButton (Some "add-point") "Add Point" "Add Point" "button" (Some "install AddPoint")
                        trailblazerButton (Some "remove-point") "Remove Point" "Remove Point" "button" (Some "install RemovePoint")
                        trailblazerButton (Some "submit-plan") "Submit Plan" "Submit Plan" "submit" None
                    ]
                    div [ _id "map"; _class "w-[50vw] h-[400px]" ] []
                    match trailPointsOfInterest with
                        | Ok _ -> emptyText
                        | Error e -> span [] [ 
                            match e with
                                | DatabaseError error -> str "An error ocurred when retrieving points of interest." 
                                | FormValidationError e -> str e
                        ]
                ]
            ]
        ] |> withMasterLayout (script [ _type "text/hyperscript"; _src "/hs/hikeplanner._hs"] [] |> Some)
