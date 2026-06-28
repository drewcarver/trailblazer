module HikePlanner.Views.Plan

open Giraffe.ViewEngine
open Giraffe

let planView =
    html [ _lang "en" ] [
        head [] [
            meta [ _charset "UTF-8" ] 
            meta [ _name "viewport"; _content "width=device-width, initial-scale=1.0" ] 
            title [] [ str "TrailForge • Plan. Hike. Remember." ]
            script [ _src "https://cdn.tailwindcss.com" ] []
            link [ _href "https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.1/css/all.min.css"; _rel "stylesheet" ] 
            style [] [
                str """
                @import url('https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600&family=Playfair+Display:wght@700&display=swap');

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
        ]
    ]

let planHandler : HttpHandler =
    htmlView planView