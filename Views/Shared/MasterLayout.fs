module HikePlanner.Views.MasterLayout

open Giraffe.ViewEngine
open HikePlanner.Core

type BodyContent = 
    | XmlNodeList of XmlNode list
    | XmlNodeBody of XmlNode

let withMasterLayout (userProfile: UserProfile option) (bodyContent: BodyContent) =
    let displayName = userProfile |> Option.map (fun p -> p.Name) |> Option.defaultValue "Guest"
    let picture = userProfile |> Option.bind (fun p -> p.Picture)

    html [ _lang "en" ] [
        head [] [
            meta [ _charset "UTF-8" ] 
            meta [ _name "viewport"; _content "width=device-width, initial-scale=1.0" ] 
            title [] [ str "Trailblazer ⛰️ Plan Your Next Hike" ]
            link [ _rel "icon"; _type "image/svg+xml"; _href "/favicon.svg" ] 
            link [ _rel "stylesheet"; _href "/css/smacss-settings.css" ]
            link [ _rel "stylesheet"; _href "/css/smacss-base.css" ]
            link [ _rel "stylesheet"; _href "/css/smacss-layout.css" ]
            link [ _rel "stylesheet"; _href "/css/smacss-modules.css" ]
            link [ _rel "stylesheet"; _href "/css/smacss-state.css" ]
            script [ _src "https://cdn.tailwindcss.com" ] []
            link [ _href "https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.1/css/all.min.css"; _rel "stylesheet" ] 
            link [ _rel "stylesheet"; _href "https://unpkg.com/leaflet@1.9.4/dist/leaflet.css"; _integrity "sha256-p4NxAoJBhIIN+hmNHrzRCf9tD/miZyoHS5obTRR9BMY="; _crossorigin ""]
            script [ _src "https://cdnjs.cloudflare.com/ajax/libs/htmx/2.0.10/htmx.min.js"; _crossorigin "anonymous"; ] []
            script [ _src "https://unpkg.com/leaflet@1.9.4/dist/leaflet.js"; _integrity "sha256-20nQCchB9co0qIjJZRGuk2/Z9VM+kNiyxNV1lvTlZBo="; _crossorigin "" ] []
            script [ _src "/js/trailmap.js"; attr "defer" ""] []
            script [ _type "text/hyperscript"; _src "/hs/autosuggest._hs"; attr "defer" "" ] []
            script [ _type "text/hyperscript"; _src "/hs/hikeplanner._hs"; attr "defer" "" ] []
            script [ _src "https://cdn.jsdelivr.net/npm/hyperscript.org@0.9.93/dist/_hyperscript.min.js"; _integrity "sha384-/6HsqTiz02YfFBUhzTwlH/yxe68DhfnkdHiWytM3nxAzs/yvG+3FZY0f4KLnNoov"; _crossorigin "anonymous" ] []
            script [ _src "https://cdn.jsdelivr.net/npm/@turf/turf@7/turf.min.js" ] []
        ]
        body [ attr "hx-boost" "true"; attr "hx-indicator" "#page-loading-overlay"; _class "site-body" ] [
            div [ _id "page-loading-overlay"; _class "site-loading-overlay"; attr "role" "status"; attr "aria-live" "polite"; attr "aria-label" "Loading next page" ] [
                div [ _class "site-loading-overlay__panel" ] [
                    div [ _class "site-loading-overlay__spinner" ] []
                    span [ _class "site-loading-overlay__text" ] [ str "Loading" ]
                ]
            ]
            nav [ _class "site-nav" ] [
                div [ _class "site-nav__inner" ] [
                    a [ _href "/"; _class "site-nav__brand" ] [ str "[TB] Trailblazer" ]
                    div [ _class "site-nav__group" ] [
                        match userProfile with
                        | Some _ ->
                            div [ _class "site-nav__user" ] [
                                a [ _href "/hikes"; _class "btn btn--compact" ] [ str "My Hikes" ]
                                match picture with
                                | Some url ->
                                    img [ _src url; _class "site-nav__avatar" ]
                                | None -> 
                                    span [ _class "site-nav__avatar site-nav__avatar-fallback" ] [ str displayName.[0..0] ]
                                span [ _class "site-nav__username" ] [ str displayName ]
                                a [ _href "/logout"; attr "hx-boost" "false"; _class "btn btn--compact" ] [ str "[ Log Out ]" ]
                            ]
                        | None ->
                            a [ _href "/login"; attr "hx-boost" "false"; _class "btn btn--compact" ] [ str "[ Log In ]" ]
                    ]
                ]
            ]    
            yield! match bodyContent with
                   | XmlNodeList nodes -> nodes
                   | XmlNodeBody node -> [node]
          ]
    ]
