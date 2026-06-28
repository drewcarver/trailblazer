module HikePlanner.Views

open Giraffe.ViewEngine
open Giraffe

let indexView =
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
            header [ _class "hero-bg h-screen flex items-center text-white" ] [
                div [ _class "max-w-4xl mx-auto px-6 text-center" ] [
                    h1 [ _class "text-6xl md:text-7xl font-bold leading-tight mb-6" ] [
                        str "Plan."
                        br []
                        str "Hike."
                        br []
                        str "Remember."
                    ]
                    p [ _class "text-xl md:text-2xl mb-10 max-w-lg mx-auto text-[#EDE4D5]" ] [
                        str "Your personal trail companion inspired by the wild beauty of Yellowstone"
                    ]
                    button [ _onclick "alert('Planning flow would open here!')"; _class "bg-[#4A7043] hover:bg-[#2E5A3D] text-white text-lg px-10 py-4 rounded-2xl font-semibold transition-all active:scale-95" ] [
                        str "Start Planning Your Next Adventure"
                    ]
                    div [ _class "mt-16 flex justify-center gap-8 text-sm" ] [
                        div [ _class "flex items-center gap-2" ] [
                            i [ _class "fa-solid fa-map" ] []
                            span [] [ str "5000+ Trails" ]
                        ]
                        div [ _class "flex items-center gap-2" ] [
                            i [ _class "fa-solid fa-users" ] []
                            span [] [ str "Community Verified" ]
                        ]
                    ]
                ]
            ]
            main [ _class "max-w-7xl mx-auto px-6 py-16" ] [
                div [ _class "grid md:grid-cols-2 gap-10" ] [
                    div [] [
                        div [ _class "flex items-center justify-between mb-6" ] [
                            h2 [ _class "text-3xl font-semibold text-[#2E5A3D]" ] [ str "Upcoming Hikes" ]
                            a [ _href "#"; _class "text-[#8B5A2B] hover:underline flex items-center gap-1 text-sm font-medium" ] [
                                str "View all "
                                i [ _class "fa-solid fa-arrow-right" ] []
                            ]
                        ]
                        div [ _class "space-y-4" ] [
                            div [ _class "bg-white rounded-3xl overflow-hidden shadow-sm border border-[#D4C3A8]" ] [
                                div [ _class "flex" ] [
                                    img [ _src "https://picsum.photos/id/1015/400/300"; _alt "Grand Prismatic Loop"; _class "w-32 h-32 object-cover" ] 
                                    div [ _class "p-5 flex-1" ] [
                                        div [ _class "flex justify-between" ] [
                                            div [] [
                                                h3 [ _class "font-semibold" ] [ str "Grand Prismatic Loop" ]
                                                p [ _class "text-sm text-gray-500" ] [ str "Yellowstone National Park • June 30" ]
                                            ]
                                            span [ _class "text-xs px-3 py-1 bg-green-100 text-green-700 rounded-full h-fit" ] [ str "Moderate" ]
                                        ]
                                        div [ _class "mt-4 flex gap-6 text-sm" ] [
                                            div [] [ span [ _class "font-mono font-bold" ] [ str "8.2" ]; str " mi" ]
                                            div [] [ span [ _class "font-mono font-bold" ] [ str "1,450" ]; str " ft" ]
                                        ]
                                    ]
                                ]
                            ]
                            div [ _class "bg-white rounded-3xl overflow-hidden shadow-sm border border-[#D4C3A8]" ] [
                                div [ _class "flex" ] [
                                    img [ _src "https://picsum.photos/id/133/400/300"; _alt "Tower Fall Trail"; _class "w-32 h-32 object-cover" ] 
                                    div [ _class "p-5 flex-1" ] [
                                        div [ _class "flex justify-between" ] [
                                            div [] [
                                                h3 [ _class "font-semibold" ] [ str "Tower Fall Trail" ]
                                                p [ _class "text-sm text-gray-500" ] [ str "Yellowstone • July 12" ]
                                            ]
                                            span [ _class "text-xs px-3 py-1 bg-amber-100 text-amber-700 rounded-full h-fit" ] [ str "Easy" ]
                                        ]
                                        div [ _class "mt-4 flex gap-6 text-sm" ] [
                                            div [] [ span [ _class "font-mono font-bold" ] [ str "4.1" ]; str " mi" ]
                                            div [] [ span [ _class "font-mono font-bold" ] [ str "820" ]; str " ft" ]
                                        ]
                                    ]
                                ]
                            ]
                        ]
                    ]
                    div [] [
                        div [ _class "flex items-center justify-between mb-6" ] [
                            h2 [ _class "text-3xl font-semibold text-[#2E5A3D]" ] [ str "Recent Hikes" ]
                            a [ _href "#"; _class "text-[#8B5A2B] hover:underline flex items-center gap-1 text-sm font-medium" ] [
                                str "View Journal "
                                i [ _class "fa-solid fa-arrow-right" ] []
                            ]
                        ]
                        div [ _class "space-y-4" ] [
                            div [ _class "bg-white rounded-3xl p-5 shadow-sm border border-[#D4C3A8] flex gap-5" ] [
                                img [ _src "https://picsum.photos/id/201/120/120"; _alt "Wapiti Lake"; _class "w-24 h-24 object-cover rounded-2xl" ] 
                                div [ _class "flex-1" ] [
                                    h3 [ _class "font-semibold" ] [ str "Wapiti Lake Trail" ]
                                    p [ _class "text-sm text-gray-500" ] [ str "June 18, 2026 • 6.8 mi" ]
                                    div [ _class "flex gap-4 mt-3 text-xs" ] [
                                        div [ _class "flex items-center gap-1" ] [
                                            i [ _class "fa-solid fa-arrow-trend-up text-[#8B5A2B]" ] []
                                            span [] [ str "1,240 ft" ]
                                        ]
                                        div [ _class "flex items-center gap-1" ] [
                                            span [ _class "text-emerald-600" ] [ str "4h 12m" ]
                                        ]
                                    ]
                                ]
                            ]
                        ]
                    ]
                ]
                div [ _class "mt-24" ] [
                    h2 [ _class "text-3xl font-semibold text-center text-[#2E5A3D] mb-12" ] [ str "Everything you need for better hikes" ]
                    div [ _class "grid md:grid-cols-3 gap-8" ] [
                        div [ _class "bg-white p-8 rounded-3xl shadow-sm border border-[#D4C3A8]" ] [
                            div [ _class "w-12 h-12 bg-[#4A7043] text-white rounded-2xl flex items-center justify-center mb-6" ] [
                                i [ _class "fa-solid fa-map text-2xl" ] []
                            ]
                            h3 [ _class "text-2xl font-semibold mb-2" ] [ str "Trail Planner" ]
                            p [ _class "text-gray-600" ] [ str "Build custom routes, check difficulty, elevation, and weather conditions." ]
                        ]
                        div [ _class "bg-white p-8 rounded-3xl shadow-sm border border-[#D4C3A8]" ] [
                            div [ _class "w-12 h-12 bg-[#8B5A2B] text-white rounded-2xl flex items-center justify-center mb-6" ] [
                                i [ _class "fa-solid fa-book text-2xl" ] []
                            ]
                            h3 [ _class "text-2xl font-semibold mb-2" ] [ str "Hike Journal" ]
                            p [ _class "text-gray-600" ] [ str "Log photos, thoughts, wildlife sightings, and track your progress over time." ]
                        ]
                        div [ _class "bg-white p-8 rounded-3xl shadow-sm border border-[#D4C3A8]" ] [
                            div [ _class "w-12 h-12 bg-[#A67C5D] text-white rounded-2xl flex items-center justify-center mb-6" ] [
                                i [ _class "fa-solid fa-users text-2xl" ] []
                            ]
                            h3 [ _class "text-2xl font-semibold mb-2" ] [ str "Community" ]
                            p [ _class "text-gray-600" ] [ str "Share trips, get recommendations, and connect with fellow hikers." ]
                        ]
                    ]
                ]
            ]
            footer [ _class "bg-[#2E5A3D] text-[#EDE4D5] py-16" ] [
                div [ _class "max-w-7xl mx-auto px-6" ] [
                    div [ _class "flex flex-col md:flex-row justify-between items-center gap-8" ] [
                        div [ _class "flex items-center gap-3" ] [
                            i [ _class "fa-solid fa-mountain text-4xl" ] []
                            div [ _class "logo-font text-4xl font-bold" ] [ str "TrailForge" ]
                        ]
                        div [ _class "text-center md:text-right" ] [
                            p [ _class "text-sm opacity-75" ] [ str "Made for those who love the trails" ]
                            p [ _class "text-xs mt-6 opacity-50" ] [ str "© 2026 TrailForge • Not affiliated with Yellowstone National Park" ]
                        ]
                    ]
                ]
            ]
        ]
    ]

let homeHandler: HttpHandler =
    htmlView indexView