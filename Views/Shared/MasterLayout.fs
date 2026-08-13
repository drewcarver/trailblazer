module HikePlanner.Views.MasterLayout

open Giraffe.ViewEngine
open HikePlanner.Core

type BodyContent = 
    | XmlNodeList of XmlNode list
    | XmlNodeBody of XmlNode

let withMasterLayout (userProfile: UserProfile option) (bodyContent: BodyContent) =
    let displayName = userProfile |> Option.map (fun p -> p.Name) |> Option.defaultValue "User"
    let picture = userProfile |> Option.bind (fun p -> p.Picture)

    html [ _lang "en" ] [
        head [] [
            meta [ _charset "UTF-8" ]
            meta [ _name "viewport"; _content "width=device-width, initial-scale=1.0" ]
            title [] [ str "Trailblazer - Plan Your Next Hike" ]
            link [ _rel "icon"; _type "image/svg+xml"; _href "/favicon.svg" ]
            link [ _rel "preconnect"; _href "https://fonts.googleapis.com" ]
            link [ _rel "preconnect"; _href "https://fonts.gstatic.com"; attr "crossorigin" "" ]
            link [ _href "https://fonts.googleapis.com/css2?family=Manrope:wght@400;500;600;700;800&display=swap"; _rel "stylesheet" ]
            link [ _rel "stylesheet"; _href "/css/index.css" ]
            link [ _href "https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.1/css/all.min.css"; _rel "stylesheet" ]
            link [ _rel "stylesheet"; _href "https://unpkg.com/leaflet@1.9.4/dist/leaflet.css"; _integrity "sha256-p4NxAoJBhIIN+hmNHrzRCf9tD/miZyoHS5obTRR9BMY="; _crossorigin "" ]
            script [ _src "https://cdnjs.cloudflare.com/ajax/libs/htmx/2.0.10/htmx.min.js"; _crossorigin "anonymous" ] []
            script [ _src "https://unpkg.com/leaflet@1.9.4/dist/leaflet.js"; _integrity "sha256-20nQCchB9co0qIjJZRGuk2/Z9VM+kNiyxNV1lvTlZBo="; _crossorigin "" ] []
            script [ _src "/js/trailmap.js"; attr "defer" "" ] []
            script [ _type "text/hyperscript"; _src "/hs/autosuggest._hs"; attr "defer" "" ] []
            script [ _type "text/hyperscript"; _src "/hs/hikeplanner._hs"; attr "defer" "" ] []
            script [ _src "https://cdn.jsdelivr.net/npm/hyperscript.org@0.9.93/dist/_hyperscript.min.js"; _integrity "sha384-/6HsqTiz02YfFBUhzTwlH/yxe68DhfnkdHiWytM3nxAzs/yvG+3FZY0f4KLnNoov"; _crossorigin "anonymous" ] []
            script [ _src "https://cdn.jsdelivr.net/npm/@turf/turf@7/turf.min.js" ] []
        ]
        body [ attr "hx-boost" "true"; attr "hx-indicator" "#page-loading-overlay" ] [
            div [ _id "page-loading-overlay"; _class "site-loading-overlay"; attr "role" "status"; attr "aria-live" "polite"; attr "aria-label" "Loading next page" ] [
                div [ _class "site-loading-overlay__panel" ] [
                    div [ _class "site-loading-overlay__spinner" ] []
                    span [ _class "site-loading-overlay__text" ] [ str "Loading" ]
                ]
            ]

            nav [ _class "nav" ] [
                a [ _class "nav-logo"; _href "/"; attr "aria-label" "Trailblazer home" ] [
                    img [ _src "/assets/mountain-logo.svg"; _alt "Trailblazer logo" ] 
                    str " Trailblazer"
                ]
                ul [ _class "nav-list" ] [
                    li [] [ a [ _href "/" ] [ str "Explore" ] ]
                    li [] [ a [ _href "/hikes" ] [ str "My Hikes" ] ]
                ]
                match userProfile with
                | Some _ ->
                    a [ _class "nav-logout"; _href "/logout"; attr "hx-boost" "false"; attr "aria-label" "User profile menu" ] [
                        i [ _class "nav-user-profile fa-regular fa-circle-user" ] []
                        str " "
                        match picture with
                        | Some url ->
                            span [] [ str displayName ]
                        | None ->
                            str displayName
                        str " "
                        i [ _class "nav-user-profile-chevron fa-solid fa-angle-down" ] []
                    ]
                | None ->
                    a [ _class "nav-logout"; _href "/login"; attr "hx-boost" "false"; attr "aria-label" "User profile menu" ] [
                        i [ _class "nav-user-profile fa-regular fa-circle-user" ] []
                        str " User "
                        i [ _class "nav-user-profile-chevron fa-solid fa-angle-down" ] []
                    ]
            ]

            yield! match bodyContent with
                   | XmlNodeList nodes -> nodes
                   | XmlNodeBody node -> [ node ]
        ]
    ]
