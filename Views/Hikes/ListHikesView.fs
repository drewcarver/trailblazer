module HikePlanner.Views.Hikes.ListHikesView

open HikePlanner.Views.MasterLayout
open HikePlanner.Views.Components.Table
open Giraffe.ViewEngine

let listHikesView userProfile =
    let table = div[] [
        div [ 
            _id "hikes-table-container" 
            attr "hx-get" "/hikes/list" 
            attr "hx-trigger" "load"
            attr "hx-swap" "outerHTML"
            attr "hx-target" "#hikes-table-container"
            attr "hx-indicator" "#hikes-table-loading-skeleton"
        ] []
        trailblazerSkeletonTable "My Hikes" 4 10 [
            _id "hikes-table-loading-skeleton"
        ]
    ]

    withMasterLayout userProfile (XmlNodeBody table)
