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
        body [ attr "hx-boost" "true"; _class "font-mono" ] [
            nav [ _class "my-2 mx-1 border-2 rounded-md border-black bg-white" ] [
                div [ _class "items-center px-4 py-3 flex justify-between" ] [
                    span [ _class "text-2xl font-bold" ] [ str "[TB] Trailblazer" ]
                    div [ _class "flex items-center gap-6 text-lg" ] [
                        a [ _href "#"; _class "tracking-tight text-[1.35rem] hover:underline" ] [ str "Find Trails" ]
                        a [ _href "/hikes"; _class "tracking-tight text-[1.35rem] hover:underline" ] [ str "My Hikes" ]
                        a [ _href "/hikes/create"; _class "tracking-tight text-[1.35rem] hover:underline" ] [ str "Create Hike" ]
                        a [ _href "/login"; attr "hx-boost" "false"; _class "tracking-tight text-[1.35rem] hover:bg-black hover:text-white transition-all" ] [ str "[ Log In ]" ]
                    ]
                ]
            ]    
            yield! match bodyContent with
                   | XmlNodeList nodes -> nodes
                   | XmlNodeBody node -> [node]
          ]
    ]
