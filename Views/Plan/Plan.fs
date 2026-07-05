module HikePlanner.Views.Plan

open Giraffe.ViewEngine
open Giraffe
open HikePlanner.Views.Components.TextInput
open HikePlanner.Views.Components.DatePicker
open HikePlanner.Views.MasterLayout

let planView =
        div [] [
            div [ _id "map"; _class "w-[90vw] h-[400px]" ] []
            form [ _class "max-w-3xl mx-auto mt-8 p-6 bg-white rounded-3xl shadow-md border border-[#D4C3A8]"; attr "hx-post" "/plan" ] [
                textInput "trail-name" "trailName" "Trail Name"
                datePicker "start-date" "Start Date" "Start Date"
                datePicker "end-date" "End Date" "End Date"
                button [ _type "submit"; _class "bg-[#4A7043] hover:bg-[#2E5A3D] text-white px-6 py-2 rounded-full font-medium transition-colors" ] [ str "Submit Plan" ]
            ]
        ] |> withMasterLayout

let planHandler : HttpHandler =
    htmlView planView
