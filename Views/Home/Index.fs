module HikePlanner.Views.Home

open Giraffe.ViewEngine
open MasterLayout
open HikePlanner.Views.Components.Card

let indexView userProfile =
    [
        main [ _class "l-site-main" ] [
            header [ _class "hero" ] [
                h1 [ _class "hero-title" ] [ str "Plan Your Next Hike" ]
                img [ _src "/assets/mountains.svg"; _alt "A scenic mountain view"; _class "hero-image" ]
            ]
            section [ _class "l-hero-content" ] [
                tbCard "" [
                    h2 [ _class "card-title" ] [ str "Upcoming Hikes" ]
                    div [] [ 
                        p [] [ str "May 15: Mt. Rainier" ]
                    ]
                    p [ _class "card-text" ] [ str "Explore a variety of trails and plan your next adventure." ]
                ]
                tbCard "" [
                    h2 [ _class "card-title" ] [ str "Popular Trails" ]
                    p [ _class "card-text" ] [ str "Explore a variety of trails and plan your next adventure." ]
                ]
                tbCard "" [
                    h2 [ _class "card-title" ] [ str "Discover Trails" ]
                    p [ _class "card-text" ] [ str "Explore a variety of trails and plan your next adventure." ]
                ]
            ]
        ]
    ] |> XmlNodeList |> withMasterLayout userProfile

