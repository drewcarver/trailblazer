module HikePlanner.Views.Home

open Giraffe.ViewEngine
open Giraffe
open MasterLayout
open HikePlanner.Views.Components.Card
open HikePlanner.Views.Components.Heading
open HikePlanner.Views.Components.Icon
open HikePlanner.Views.Components.Description
open HikePlanner.Views.Components.Button

let indexView =
    [
        main [ _class "max-w-7xl mx-auto px-6 py-12" ] [
            div [ _class "mb-16" ] [
                tbCard "text-center max-w-2xl mx-auto" [
                    tbIcon "🌲"
                    tbH2 "PLAN YOUR NEXT ADVENTURE TOGETHER"
                    tbDescription "A simple, visual tool built to map out multi-day hikes, track trails, and coordinate coordinates perfectly with friends."
                    div [ _class "mt-8" ] [
                        trailblazerButton None "Start Planning" "START PLANNING" "button" [("onclick", "window.location.href='/create-plan'")]
                    ]
                ]
            ]
            
            div [ _class "mb-16" ] [
                tbH2' "border-2 border-black rounded-lg p-4 w-fit" "HOW IT WORKS"
                div [ _class "grid md:grid-cols-3 gap-6 mt-8" ] [
                    tbCard "" [
                        tbH3 "1. Pick Your Route"
                        p [ _class "text-sm leading-relaxed" ] [ str "Select trail points on an interactive map." ]
                    ]
                    tbCard "" [
                        tbH3 "2. Invite the Crew"
                        p [ _class "text-sm leading-relaxed" ] [ str "Drop in emails to share real-time updates." ]
                    ]
                    tbCard "" [
                        tbH3 "3. Plot Day-by-Day"
                        p [ _class "text-sm leading-relaxed" ] [ str "Build your itinerary with stops and checkpoints." ]
                    ]
                ]
            ]
            
            div [ _class "grid md:grid-cols-2 gap-6 mb-16" ] [
                tbCard "" [
                    tbH2' "border-b-2 border-black pb-3" "YOUR UPCOMING TRIP PREVIEW"
                    p [ _class "text-sm mt-4" ] [ str "Springer Mountain Weekend Challenge and options." ]
                ]
                tbCard "" [
                    tbH2' "border-b-2 border-black pb-3" "3. PLOT DAY-BY-DAY"
                    tbH3 "Springer Mountain Weekend Challenge"
                    div [ _class "space-y-3 text-sm my-6" ] [
                        div [] [ span [ _class "font-bold" ] [ str "Start Date:" ]; str " 10/24/2026" ]
                        div [] [ span [ _class "font-bold" ] [ str "Group:" ]; str " 4 Friends" ]
                        div [] [ span [ _class "font-bold" ] [ str "Status:" ]; str " Active Routing" ]
                    ]
                    div [ _class "mt-8" ] [
                        trailblazerButton None "View Details" "VIEW FULL HIKE DETAILS" "button" [("onclick", "window.location.href='/hikes'")]
                    ]
                ]
            ]
        ]
        footer [ _class "bg-white border-t-2 border-black py-8 mt-16" ] [
            div [ _class "max-w-7xl mx-auto px-6 text-center" ] [
                p [ _class "text-sm font-bold" ] [ str "© 2026 Trailblazer App. Built for hikers, by hikers." ]
            ]
        ]
    ] |> XmlNodeList |> withMasterLayout None 

let homeHandler: HttpHandler =
    htmlView indexView