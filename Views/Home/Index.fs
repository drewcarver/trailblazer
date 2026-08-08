module HikePlanner.Views.Home

open System.Security.Claims
open Giraffe.ViewEngine
open Giraffe
open MasterLayout
open HikePlanner.Core
open HikePlanner.Infrastructure
open HikePlanner.Views.Components.LinkButton

let indexView userProfile =
    [
        main [ _class "site-main" ] [
            div [ _class "hero" ] [
                div [ _class "hero__panel card card--hero" ] [
                    div [ _class "hero__header" ] [
                        div [ _class "hero__eyebrow" ] [ str "🌲" ]
                        h2 [ _class "hero__title" ] [ str "Plan Your Next Hike" ]
                    ]
                    p [ _class "hero__copy" ] [
                        str "Trailblazer is a web app that helps you plan your next hiking adventure with friends."
                    ]
                    div [] [
                        trailblazerLinkButton "/hikes" "START PLANNING" (Some "btn--large")
                    ]
                ]
            ]
        ]
    ] |> XmlNodeList |> withMasterLayout userProfile

