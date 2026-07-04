module HikePlanner.Views.Plan

open Giraffe.ViewEngine
open Giraffe
open HikePlanner.Views.Components.TextInput
open HikePlanner.Views.Components.DatePicker
open HikePlanner.Views.MasterLayout

let planView =
        body [ _class "bg-[#EDE4D5] text-gray-800" ] [
            nav [ _class "bg-[#2E5A3D] text-white sticky top-0 z-50 shadow-md" ] [
                div [ _class "max-w-7xl mx-auto px-6 py-4 flex items-center justify-between" ] [
                    div [ _class "flex items-center gap-3" ] [
                        i [ _class "fa-solid fa-mountain text-3xl text-[#EDE4D5]" ] []
                        div [ _class "logo-font text-3xl font-bold tracking-tight" ] [ str "TrailForge" ]
                    ]
                    div [ _class "hidden md:flex items-center gap-8 text-sm font-medium" ] [
                        a [ _href "#"; _class "hover:text-[#D4C3A8] transition-colors" ] [ str "Discover Trails" ]
                        a [ _href "#"; _class "hover:text-[#D4C3A8] transition-colors" ] [ str "Plan Hike" ]
                        a [ _href "#"; _class "hover:text-[#D4C3A8] transition-colors" ] [ str "My Journal" ]
                        a [ _href "#"; _class "hover:text-[#D4C3A8] transition-colors" ] [ str "Past Hikes" ]
                        a [ _href "#"; _class "hover:text-[#D4C3A8] transition-colors" ] [ str "Community" ]
                    ]
                    div [ _class "flex items-center gap-4" ] [
                        button [ _class "bg-[#8B5A2B] hover:bg-[#A67C5D] px-5 py-2 rounded-full text-sm font-medium transition-colors" ] [ str "Log Hike" ]
                        div [ _class "w-9 h-9 bg-[#D4C3A8] rounded-full flex items-center justify-center text-[#2E5A3D] font-semibold" ] [ str "JD" ]
                    ]
                ]
            ]
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
