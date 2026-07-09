module HikePlanner.Views.Plan.Plan

open Giraffe.ViewEngine
open HikePlanner.Views.Components.TextInput
open HikePlanner.Views.Components.DatePicker
open HikePlanner.Views.Components.Select
open HikePlanner.Views.MasterLayout
open HikePlanner.Repositories.HikeRepo

let planView trailPointsOfInterest =
        let toOptionLabel poi = sprintf "%s - Mile %.2f" poi.Name poi.TrailMile
        div [] [
            div [ _id "map"; _class "w-[90vw] h-[400px]" ] []
            form [ _class "max-w-3xl mx-auto mt-8 p-6 bg-white rounded-3xl shadow-md border border-[#D4C3A8]"; attr "hx-post" "/plan" ] [
                textInput "trail-name" "hikeName" "Trail Name"
                datePicker "start-date" "startDate" "Start Date"
                datePicker "end-date" "endDate" "End Date"
                trailblazerSelect "start-point-select" "startPoint" "Starting Point" (
                    trailPointsOfInterest 
                    |> Seq.map (fun poi -> { Label = poi |> toOptionLabel; Value = poi.Name }))
                trailblazerSelect "end-point-select" "endPoint" "Ending Point" (
                    trailPointsOfInterest 
                    |> Seq.map (fun poi -> { Label = poi |> toOptionLabel; Value = poi.Name }))
                button [ _type "submit"; _class "bg-[#4A7043] hover:bg-[#2E5A3D] text-white px-6 py-2 rounded-full font-medium transition-colors" ] [ str "Submit Plan" ]
            ]
        ] |> withMasterLayout
