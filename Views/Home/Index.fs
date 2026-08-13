module HikePlanner.Views.Home

open Giraffe.ViewEngine
open MasterLayout

let indexView userProfile =
    [
        main [] [
            header [ _class "hero"; attr "role" "banner" ] [
                h1 [] [ str "Plan Your Next Hike" ]
                p [ _class "hero-description" ] [ str "Discover trails, connect with friends, and make every hike your best one yet." ]
                p [ _class "hero-actions" ] [
                    a [ _class "btn btn-primary"; _href "/" ] [
                        str "Explore Trails "
                        i [ _class "fa-solid fa-arrow-right" ] []
                    ]
                    a [ _class "btn btn-secondary"; _href "/hikes" ] [ str "My Hikes" ]
                ]
            ]

            section [ _class "l-cards"; attr "aria-label" "Hike highlights" ] [
                article [ _class "card" ] [
                    h2 [ _class "card-title" ] [
                        i [ _class "card-title-icon fa-regular fa-calendar" ] []
                        str " Upcoming Hikes "
                        a [ _class "card-title-view-all"; _href "/hikes" ] [
                            str "View all "
                            i [ _class "fa-solid fa-angle-right" ] []
                        ]
                    ]
                    ul [ _class "card-item-list" ] [
                        li [] [
                            a [ _class "card-item"; _href "/hike" ] [
                                time [ _class "calendar"; attr "datetime" "2026-05-15" ] [
                                    span [ _class "calendar-month" ] [ str "MAY" ]
                                    strong [ _class "calendar-day" ] [ str "15" ]
                                ]
                                div [ _class "upcoming-hike-detail" ] [
                                    h3 [ _class "hike-title" ] [ str "Mt. Adams" ]
                                    p [ _class "hike-info" ] [ str "May 15 • 8.5 mi • Moderate" ]
                                    p [ _class "hike-location" ] [
                                        i [ _class "fa-solid fa-location-dot" ] []
                                        str " Washington"
                                    ]
                                ]
                                i [ _class "card-item-chevron fa-solid fa-angle-right"; attr "aria-hidden" "true" ] []
                            ]
                        ]
                        li [] [
                            a [ _class "card-item"; _href "/hike" ] [
                                time [ _class "calendar"; attr "datetime" "2026-06-01" ] [
                                    span [ _class "calendar-month" ] [ str "JUN" ]
                                    strong [ _class "calendar-day" ] [ str "1" ]
                                ]
                                div [ _class "upcoming-hike-detail" ] [
                                    h3 [ _class "hike-title" ] [ str "Glacier Trail" ]
                                    p [ _class "hike-info" ] [ str "June 1 • 12.6 mi • Challenging" ]
                                    p [ _class "hike-location" ] [
                                        i [ _class "fa-solid fa-location-dot" ] []
                                        str " Montana"
                                    ]
                                ]
                                i [ _class "card-item-chevron fa-solid fa-angle-right"; attr "aria-hidden" "true" ] []
                            ]
                        ]
                    ]
                ]

                article [ _class "card" ] [
                    h2 [ _class "card-title" ] [
                        i [ _class "card-title-icon fa-solid fa-mountain" ] []
                        str " Popular Trails "
                        a [ _class "card-title-view-all"; _href "/trails" ] [
                            str "View all "
                            i [ _class "fa-solid fa-angle-right" ] []
                        ]
                    ]
                    ul [ _class "card-item-list" ] [
                        li [] [
                            a [ _class "card-item"; _href "/trails" ] [
                                img [ _class "trail-photo"; _src "https://images.unsplash.com/photo-1511497584788-876760111969?auto=format&fit=crop&w=180&q=80"; _alt "Angel's Landing trail view" ] 
                                div [ _class "upcoming-hike-detail" ] [
                                    h3 [ _class "hike-title" ] [ str "Angel's Landing" ]
                                    p [ _class "hike-info" ] [
                                        str "Zion National Park "
                                        span [ _class "hike-difficulty-chip hard" ] [ str "Hard" ]
                                    ]
                                ]
                                i [ _class "card-item-chevron fa-solid fa-angle-right"; attr "aria-hidden" "true" ] []
                            ]
                        ]
                        li [] [
                            a [ _class "card-item"; _href "/trails" ] [
                                img [ _class "trail-photo"; _src "https://images.unsplash.com/photo-1464822759023-fed622ff2c3b?auto=format&fit=crop&w=180&q=80"; _alt "Zion Narrows canyon trail" ] 
                                div [ _class "upcoming-hike-detail" ] [
                                    h3 [ _class "hike-title" ] [ str "Zion Narrows" ]
                                    p [ _class "hike-info" ] [
                                        str "Zion National Park "
                                        span [ _class "hike-difficulty-chip moderate" ] [ str "Moderate" ]
                                    ]
                                ]
                                i [ _class "card-item-chevron fa-solid fa-angle-right"; attr "aria-hidden" "true" ] []
                            ]
                        ]
                    ]
                ]

                article [ _class "card" ] [
                    h2 [ _class "card-title" ] [
                        i [ _class "card-title-icon fa-solid fa-users" ] []
                        str " Friend Hikes "
                        a [ _class "card-title-view-all"; _href "/hikes" ] [
                            str "View all "
                            i [ _class "fa-solid fa-angle-right" ] []
                        ]
                    ]
                    ul [ _class "card-item-list" ] [
                        li [] [
                            a [ _class "card-item"; _href "/hikes" ] [
                                img [ _class "friend-photo"; _src "https://images.unsplash.com/photo-1500648767791-00dcc994a43e?auto=format&fit=crop&w=120&q=80"; _alt "Alex profile photo" ] 
                                div [ _class "upcoming-hike-detail" ] [
                                    h3 [ _class "hike-title" ] [ str "Alex hiked Mt. Hood" ]
                                    p [ _class "hike-info" ] [ str "April 28 • 9.4 mi" ]
                                ]
                                i [ _class "card-item-chevron fa-solid fa-angle-right"; attr "aria-hidden" "true" ] []
                            ]
                        ]
                        li [] [
                            a [ _class "card-item"; _href "/hikes" ] [
                                img [ _class "friend-photo"; _src "https://images.unsplash.com/photo-1494790108377-be9c29b29330?auto=format&fit=crop&w=120&q=80"; _alt "Sarah profile photo" ]
                                div [ _class "upcoming-hike-detail" ] [
                                    h3 [ _class "hike-title" ] [ str "Sarah hiked Old Rag" ]
                                    p [ _class "hike-info" ] [ str "April 26 • 8.1 mi" ]
                                ]
                                i [ _class "card-item-chevron fa-solid fa-angle-right"; attr "aria-hidden" "true" ] []
                            ]
                        ]
                    ]
                ]
            ]

            section [ _class "features"; attr "aria-label" "Trailblazer benefits" ] [
                article [] [
                    i [ _class "feature-icon fa-regular fa-map" ] []
                    div [] [
                        h3 [ _class "feature-title" ] [ str "Discover Trails" ]
                        p [] [ str "Find the perfect trail for your next adventure." ]
                    ]
                ]
                article [] [
                    i [ _class "feature-icon fa-solid fa-user-group" ] []
                    div [] [
                        h3 [ _class "feature-title" ] [ str "Plan Together" ]
                        p [] [ str "Coordinate hikes with friends easily." ]
                    ]
                ]
                article [] [
                    i [ _class "feature-icon fa-solid fa-mountain-city" ] []
                    div [] [
                        h3 [ _class "feature-title" ] [ str "Track Progress" ]
                        p [] [ str "Log your hikes and reach new summits." ]
                    ]
                ]
                article [] [
                    i [ _class "feature-icon fa-regular fa-shield-heart" ] []
                    div [] [
                        h3 [ _class "feature-title" ] [ str "Stay Safe" ]
                        p [] [ str "Get trail info and weather updates in real time." ]
                    ]
                ]
            ]
        ]
    ] |> XmlNodeList |> withMasterLayout userProfile

