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
            script [ _src "https://cdn.tailwindcss.com" ] []
            script [] [
                rawText """
                window.tailwind.config = {
                    plugins: [
                    function({ addVariant }) {
                        addVariant('user-invalid', '&:user-invalid')
                        addVariant('peer-user-invalid', '.peer:user-invalid ~ &')
                    }
                    ]
                }
                                """
            ]
            link [ _href "https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.1/css/all.min.css"; _rel "stylesheet" ] 
            link [ _rel "stylesheet"; _href "https://unpkg.com/leaflet@1.9.4/dist/leaflet.css"; _integrity "sha256-p4NxAoJBhIIN+hmNHrzRCf9tD/miZyoHS5obTRR9BMY="; _crossorigin ""]
            script [ _src "https://cdnjs.cloudflare.com/ajax/libs/htmx/2.0.10/htmx.min.js"; _crossorigin "anonymous"; ] []
            script [ _src "https://unpkg.com/leaflet@1.9.4/dist/leaflet.js"; _integrity "sha256-20nQCchB9co0qIjJZRGuk2/Z9VM+kNiyxNV1lvTlZBo="; _crossorigin "" ] []
            script [ _src "/js/trailmap.js"; attr "defer" ""] []
            script [ _type "text/hyperscript"; _src "/hs/autosuggest._hs"; attr "defer" "" ] []
            script [ _type "text/hyperscript"; _src "/hs/hikeplanner._hs"; attr "defer" "" ] []
            script [ _src "https://cdn.jsdelivr.net/npm/hyperscript.org@0.9.93/dist/_hyperscript.min.js"; _integrity "sha384-/6HsqTiz02YfFBUhzTwlH/yxe68DhfnkdHiWytM3nxAzs/yvG+3FZY0f4KLnNoov"; _crossorigin "anonymous" ] []
            script [ _src "https://cdn.jsdelivr.net/npm/@turf/turf@7/turf.min.js" ] []
            style [] [
                str """
                @import url('https://fonts.googleapis.com/css2?family=IBM+Plex+Mono:ital,wght@0,100;0,200;0,300;0,400;0,500;0,600;0,700;1,100;1,200;1,300;1,400;1,500;1,600;1,700&display=swap');
                @import url('https://fonts.googleapis.com/css2?family=IBM+Plex+Mono:ital,wght@0,100;0,200;0,300;0,400;0,500;0,600;0,700;1,100;1,200;1,300;1,400;1,500;1,600;1,700&family=Roboto+Mono:ital,wght@0,100..700;1,100..700&display=swap');

                
                :root {
                    --green-dark: #2E5A3D;
                    --green-med: #4A7043;
                    --brown: #8B5A2B;
                    --beige: #EDE4D5;
                }

                * {
                    font-family: "IBM Plex Mono", "Courier New", Courier, monospace;
                }

                body {
                    background-color: var(--beige);
                }

                #page-loading-overlay {
                    position: fixed;
                    inset: 0;
                    z-index: 50;
                    display: flex;
                    align-items: center;
                    justify-content: center;
                    background: rgba(0, 0, 0, 0.25);
                    backdrop-filter: blur(2px);
                    opacity: 0;
                    pointer-events: none;
                    transition: opacity 150ms ease-in-out;
                }

                #page-loading-overlay.htmx-request {
                    opacity: 1;
                    pointer-events: auto;
                }

                .hero-bg {
                    background-image: linear-gradient(rgba(0, 0, 0, 0.45), rgba(0, 0, 0, 0.45)),
                                      url(https://picsum.photos/id/1015/2000/1200);
                    background-size: cover;
                    background-position: center;
                }

                @utility peer-user-invalid {
                    .peer:user-invalid ~ & {
                        @apply block; /* or whatever display behavior you prefer */
                    }
                }
                """
            ]
        ]
        body [ attr "hx-boost" "true"; attr "hx-indicator" "#page-loading-overlay"; _class "font-mono" ] [
            div [ _id "page-loading-overlay"; _class "htmx-indicator"; attr "role" "status"; attr "aria-live" "polite"; attr "aria-label" "Loading next page" ] [
                div [ _class "flex flex-col items-center gap-4 rounded-2xl border-2 border-black bg-white px-8 py-6 shadow-[6px_6px_0px_0px_rgba(0,0,0,1)]" ] [
                    div [ _class "h-12 w-12 animate-spin rounded-full border-4 border-black/20 border-t-black" ] []
                    span [ _class "text-sm font-bold uppercase tracking-[0.2em]" ] [ str "Loading" ]
                ]
            ]
            nav [ _class "my-2 mx-1 border-2 rounded-md border-black bg-white" ] [
                div [ _class "items-center px-4 py-3 flex justify-between" ] [
                    a [ _href "/"; _class "text-2xl font-bold" ] [ str "[TB] Trailblazer" ]
                    div [ _class "flex items-center gap-6 text-lg" ] [
                        match userProfile with
                        | Some _ ->
                            div [ _class "flex items-center gap-3" ] [
                                a [ _href "/hikes"; _class "tracking-tight text-[1.35rem] hover:bg-black hover:text-white transition-all" ] [ str "My Hikes" ]
                                match picture with
                                | Some url ->
                                    img [ _src url; _class "w-8 h-8 rounded-full" ]
                                | None -> 
                                    span [ _class "w-8 h-8 rounded-full bg-gray-300 flex items-center justify-center text-sm" ] [ str displayName.[0..0] ]
                                span [ _class "tracking-tight text-[1.35rem]" ] [ str displayName ]
                                a [ _href "/logout"; attr "hx-boost" "false"; _class "tracking-tight text-[1.35rem] hover:bg-black hover:text-white transition-all" ] [ str "[ Log Out ]" ]
                            ]
                        | None ->
                            a [ _href "/login"; attr "hx-boost" "false"; _class "tracking-tight text-[1.35rem] hover:bg-black hover:text-white transition-all" ] [ str "[ Log In ]" ]
                    ]
                ]
            ]    
            yield! match bodyContent with
                   | XmlNodeList nodes -> nodes
                   | XmlNodeBody node -> [node]
          ]
    ]
