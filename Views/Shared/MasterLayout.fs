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
            link [ _href "https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.1/css/all.min.css"; _rel "stylesheet" ] 
            link [ _rel "stylesheet"; _href "https://unpkg.com/leaflet@1.9.4/dist/leaflet.css"; _integrity "sha256-p4NxAoJBhIIN+hmNHrzRCf9tD/miZyoHS5obTRR9BMY="; _crossorigin ""]
            script [ _src "https://cdnjs.cloudflare.com/ajax/libs/htmx/2.0.10/htmx.min.js"; _integrity "sha512-mwXO+qVbheglD8l/LGeVBnqcKl9NtchGWmM9gW/gvAEZBYnsBQCpaneQ+hI+MOlv7Komhd1NqZ5Gv1ElbYgqCA=="; _crossorigin "anonymous"; ] []
            script [ _src ""; _integrity "sha256-20nQCchB9co0qIjJZRGuk2/Z9VM+kNiyxNV1lvTlZBo="; _crossorigin "" ] []
            script [ _src "https://unpkg.com/leaflet@1.9.4/dist/leaflet.js"; _integrity "sha256-20nQCchB9co0qIjJZRGuk2/Z9VM+kNiyxNV1lvTlZBo="; _crossorigin "" ] []
            script [ _src "/js/trailmap.js"; attr "defer" ""] []
            script [ _type "text/hyperscript"; _src "/hs/autosuggest._hs"; attr "defer" "" ] []
            script [ _type "text/hyperscript"; _src "/hs/hikeplanner._hs"; attr "defer" "" ] []
            script [ _src "https://cdn.jsdelivr.net/npm/hyperscript.org@0.9.93/dist/_hyperscript.min.js"; _integrity "sha384-/6HsqTiz02YfFBUhzTwlH/yxe68DhfnkdHiWytM3nxAzs/yvG+3FZY0f4KLnNoov"; _crossorigin "anonymous" ] []
            script [ _src "https://cdn.jsdelivr.net/npm/@turf/turf@7/turf.min.js" ] []
            style [] [
                str """
                @import url(https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600&family=Playfair+Display:wght@700&display=swap);

                :root {
                    --green-dark: #2E5A3D;
                    --green-med: #4A7043;
                    --brown: #8B5A2B;
                    --beige: #EDE4D5;
                }

                body {
                    font-family: 'Inter', system-ui, sans-serif;
                }

                .logo-font {
                    font-family: 'Playfair Display', sans-serif;
                }

                .hero-bg {
                    background-image: linear-gradient(rgba(0, 0, 0, 0.45), rgba(0, 0, 0, 0.45)),
                                      url(https://picsum.photos/id/1015/2000/1200);
                    background-size: cover;
                    background-position: center;
                }
                """
            ]
        ]
        body [ attr "hx-boost" "true"; _class "bg-[#EDE4D5] text-gray-800" ] [
            nav [ _class "bg-[#2E5A3D] text-white sticky top-0 z-50 shadow-md" ] [
                div [ _class "max-w-7xl mx-auto px-6 py-4 flex items-center justify-between" ] [
                    div [ _class "flex items-center gap-3" ] [
                        i [ _class "fa-solid fa-mountain text-3xl text-[#EDE4D5]" ] []
                        div [ _class "logo-font text-3xl font-bold tracking-tight" ] [ str "TrailBlazer" ]
                    ]
                    div [ _class "hidden md:flex items-center gap-8 text-sm font-medium" ] [
                        a [ _href "#"; _class "hover:text-[#D4C3A8] transition-colors" ] [ str "Discover Trails" ]
                        a [ _href "/hikes"; _class "hover:text-[#D4C3A8] transition-colors" ] [ str "Plan Hike" ]
                        a [ _href "#"; _class "hover:text-[#D4C3A8] transition-colors" ] [ str "My Journal" ]
                        a [ _href "#"; _class "hover:text-[#D4C3A8] transition-colors" ] [ str "Past Hikes" ]
                        a [ _href "#"; _class "hover:text-[#D4C3A8] transition-colors" ] [ str "Community" ]
                    ]
                    div [ _class "flex items-center gap-4" ] [
                        button [ _class "bg-[#8B5A2B] hover:bg-[#A67C5D] px-5 py-2 rounded-full text-sm font-medium transition-colors" ] [ str "Log Hike" ]
                        div [ _class "flex items-center gap-2 px-2 h-9 bg-[#D4C3A8] rounded-full" ] [
                            match picture with
                            | Some src -> img [ _src src; _alt displayName; _class "w-7 h-7 rounded-full object-cover" ]
                            | None -> emptyText
                            span [ _class "pr-2 text-[#2E5A3D] font-semibold text-sm whitespace-nowrap" ] [ str displayName ]
                        ]
                        a [ _href "/logout"; attr "hx-boost" "false"; _class "text-sm font-medium hover:text-[#D4C3A8] transition-colors" ] [ str "Log Out" ]
                    ]
                ]
            ]    
            yield! match bodyContent with
                   | XmlNodeList nodes -> nodes
                   | XmlNodeBody node -> [node]
          ]
    ]
