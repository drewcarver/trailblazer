module HikePlanner.Views.Home

open System.Security.Claims
open Giraffe.ViewEngine
open Giraffe
open MasterLayout
open HikePlanner.Core
open HikePlanner.Infrastructure

let indexView userProfile =
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
        ]
    ] |> XmlNodeList |> withMasterLayout userProfile

