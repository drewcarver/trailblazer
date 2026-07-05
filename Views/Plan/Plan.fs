module HikePlanner.Views.Plan.Plan

open Giraffe.ViewEngine
open HikePlanner.Views.Components.TextInput
open HikePlanner.Views.Components.DatePicker
open HikePlanner.Views.MasterLayout
open HikePlanner.Repositories.HikeRepo

let planView trailPointsOfInterest =
        div [] [
            div [ _id "map"; _class "w-[90vw] h-[400px]" ] []
            form [ _class "max-w-3xl mx-auto mt-8 p-6 bg-white rounded-3xl shadow-md border border-[#D4C3A8]"; attr "hx-post" "/plan" ] [
                textInput "trail-name" "trailName" "Trail Name"
                datePicker "start-date" "Start Date" "Start Date"
                datePicker "end-date" "End Date" "End Date"
                div [ _class "mb-4" ] [
                    label [ _for "points-of-interest"; _class "block text-sm font-medium text-gray-700 mb-1" ] [ str "Points of Interest" ]
                    select [ _id "points-of-interest"; _name "pointsOfInterest"; _class "w-full border border-gray-300 rounded-md shadow-sm focus:ring focus:ring-opacity-50 focus:ring-indigo-500 focus:border-indigo-500" ] [
                        for poi in trailPointsOfInterest do
                            option [ _value (sprintf "%s|%f" poi.TrailName poi.TrailMile) ] [ str (sprintf "%s - %.2f miles" poi.Name poi.TrailMile) ]
                    ]
                ]
                button [ _type "submit"; _class "bg-[#4A7043] hover:bg-[#2E5A3D] text-white px-6 py-2 rounded-full font-medium transition-colors" ] [ str "Submit Plan" ]
            ]
        ] |> withMasterLayout
