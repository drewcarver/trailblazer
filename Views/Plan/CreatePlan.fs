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
                    form [ _class "font-mono mx-auto mt-8 transition-[width] duration-500 ease-in-out"; attr "hx-post" "/plan" ] [
                        textInput "hike-name" "hikeName" "Hike Name" true
                        datePicker "start-date" "startDate" "Start Date" (Some "
                            on change or load
                                set #end-date.min to my value
                        ") (Some DateTime.Now)
                        datePicker "end-date" "endDate" "End Date" (Some "
                            on change or load
                                set #start-date.max to my value
                        ") (Some DateTime.Now)
                        trailblazerSelect "start-point-select" "startPointId" "Starting Point" pointsOfInterestOptions (Some "
                            on change 
                                set startMile to parseFloat(my selectedOptions.dataset.mile)
                
                                for opt in #end-point-select.options
                                    set endMile to parseFloat(opt.dataset.mile)

                                    if endMile < startMile
                                        set opt.disabled to true
                                    else
                                        set opt.disabled to false
                                    end
                                end
                
                                if #end-point-select.selectedOptions[0].disabled
                                    set #end-point-select.value to ''
                        ") []
                        match trailPointsOfInterest with
                            | Ok _ -> emptyText
                            | Error e -> span [] [ 
                                match e with
                                    | DatabaseError error -> str "An error ocurred when retrieving points of interest." 
                                    | _ -> emptyText
                            ]
                        trailblazerSelect "end-point-select" "endPointId" "Ending Point" pointsOfInterestOptions (Some "
                            on change 
                                set endMile to parseFloat(my selectedOptions.dataset.mile)
                
                                for opt in #start-point-select.options
                                    set startMile to parseFloat(opt.dataset.mile)

                                    if endMile < startMile
                                        set opt.disabled to true
                                    else
                                        set opt.disabled to false
                                    end
                                end
                
                                if #start-point-select.selectedOptions[0].disabled
                                    set #start-point-select.value to ''
                        ") []
                        match trailPointsOfInterest with
                            | Ok _ -> emptyText
                            | Error e -> span [] [ 
                                match e with
                                    | FormValidationError error -> str error
                                    | _ -> str "An error has occurred"
                            ]
                        trailblazerButton "Add Point" "Add Point" "button" (Some
                            """on click
                                set $clone to #end-point-select.cloneNode(true) 
                                then remove @id from $clone 
                                then put $clone after #end-point-select
                            """)
                        trailblazerButton "Submit Plan" "Submit Plan" "submit" None
                    ]
                    div [ _id "map"; _class "w-[50vw] h-[400px]" ] []
                ]
            ]
        ] |> withMasterLayout
