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
        main [ _class "w-full px-1" ] [
            div [ _class "mb-16" ] [
                div [ _class "border-2 border-black rounded-lg p-8 bg-white flex flex-col items-center gap-2" ] [
                    div [] [
                        div [ _class "flex items-center justify-center gap-2 text-[2.7rem]" ] [
                            div [] [ str "🌲" ]
                            h2 [ _class "font-bold uppercase" ] [ str "Plan Your Next Hike" ]
                        ]
                        p [ _class "mb-2 text-[1.35rem] max-w-[600px] text-center tracking-tight" ] [
                            str "Trailblazer is a web app that helps you plan your next hiking adventure with friends."
                        ]
                    ]
                    div [] [
                        a [
                            _href "/hikes"
                            _class "text-[1.5rem] inline-flex items-center justify-center px-4 py-2 font-bold uppercase border border-black bg-neutral-100 hover:bg-black hover:text-white shadow-[6px_6px_0px_0px_rgba(0,0,0,1)] active:translate-x-[1px] active:translate-y-[1px] active:shadow-[1px_1px_0px_0px_rgba(0,0,0,1)] transition-all cursor-pointer disabled:opacity-50 disabled:pointer-events-none disabled:shadow-none"
                            attr "label" "Start Planning"
                        ] [ str "START PLANNING" ]
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