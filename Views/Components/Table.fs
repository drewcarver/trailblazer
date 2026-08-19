module HikePlanner.Views.Components.Table

open Giraffe.ViewEngine
open FSharp.Core
open Button
open LinkButton

let trailblazerTableButton label text = 
  td [ _class "table__action-cell" ] [
    trailblazerButton None label text "Button" []
  ]

type ColumnValue =
    | StringValue of string
    | XmlNodeValue of XmlNode
let trailblazerTableColumn (value: ColumnValue) =
  td [] [ match value with | StringValue s -> str s | XmlNodeValue n -> n ]

let trailblazerTableRow cols = 
  tr [] cols
  

let trailblazerTableHeader headers =
    thead [] [
    tr [] [
      yield! headers |> List.map (fun header ->
        th [ _scope "col" ] [
          str header
          i [ _class "fa-solid fa-arrows-up-down"; attr "aria-hidden" "true" ] []
        ])
        ]
    ]

let trailblazerTable title tableHeader rows  =
  section [ _class "l-list-page" ] [
  div [ _class "card card--data-table" ] [
    header [ _class "l-list-page-header" ] [
      div [] [
        h1 [ _class "page-heading" ] [ str title ]
        p [ _class "description" ] [ str "View, manage, and edit all of your upcoming and past hikes." ]
      ]
      a [ _class "btn btn-primary btn--compact"; _href "/hikes/create" ] [
        i [ _class "fa-solid fa-plus"; attr "aria-hidden" "true" ] []
        str " Add Hike"
      ]
        ]
    div [ _class "table-toolbar" ] [
      nav [ _class "filter-tabs"; attr "aria-label" "Hike filters" ] [
        a [ _class "filter-tab is-active"; _href "#all-hikes" ] [ str "All Hikes" ]
        a [ _class "filter-tab"; _href "#upcoming-hikes" ] [ str "Upcoming" ]
        a [ _class "filter-tab"; _href "#past-hikes" ] [ str "Past" ]
        a [ _class "filter-tab"; _href "#friends-hikes" ] [ str "Friends" ]
      ]
      div [ _class "table-tools" ] [
        label [ _class "search"; _for "hike-search" ] [
          i [ _class "fa-solid fa-magnifying-glass"; attr "aria-hidden" "true" ] []
          input [ _id "hike-search"; _type "search"; _placeholder "Search hikes..." ]
        ]
        button [ _class "filter"; _type "button" ] [
          i [ _class "fa-solid fa-sliders"; attr "aria-hidden" "true" ] []
          str " Filters"
        ]
      ]
    ]
    div [ _class "table-wrap" ] [
      table [ _class "table table--data" ] [
        tableHeader
        tbody [] rows
      ]
    ]
    footer [ _class "table-footer" ] [
      span [] [ str "Showing 1 to 6 of 12 hikes" ]
      nav [ _class "table-pagination"; attr "aria-label" "Hike pages" ] [
        a [ _class "table-pagination-btn"; _href "#previous"; attr "aria-label" "Previous page" ] [ i [ _class "fa-solid fa-angle-left"; attr "aria-hidden" "true" ] [] ]
        a [ _class "table-pagination-btn is-active"; _href "#page-1"; attr "aria-current" "page" ] [ str "1" ]
        a [ _class "table-pagination-btn"; _href "#page-2" ] [ str "2" ]
        a [ _class "table-pagination-btn"; _href "#next"; attr "aria-label" "Next page" ] [ i [ _class "fa-solid fa-angle-right"; attr "aria-hidden" "true" ] [] ]
      ]
    ]
  ]
  ]

let private skeletonHeaderCell =
  th [ _scope "col"; _class "px-4 py-3 border-r border-black w-1/5" ] [
    div [ _class "h-4 w-24 rounded bg-neutral-300 animate-pulse" ] []
  ]

let private skeletonTableHeader columnCount =
  thead [] [
    tr [ _class "border border-black bg-neutral-100" ] [
      for _ in 1 .. columnCount do
        skeletonHeaderCell
    ]
  ]

let private skeletonTableCell =
  td [ _class "h-[3rem] px-4 py-3 border-r border-black" ] [
    div [ _class "h-4 w-full rounded bg-neutral-300 animate-pulse" ] []
  ]

let private skeletonTableRow columnCount =
  tr [ _class "hover:bg-neutral-50 transition-colors" ] [
    for _ in 1 .. columnCount do
      skeletonTableCell
  ]

let trailblazerSkeletonTable title columnCount rowCount attrs =
  div ( [ _class "htmx-indicator w-full overflow-x-auto border border-black rounded-lg bg-white p-4 font-sans selection:bg-neutral-200" ] @ attrs) [
    div [ _class "flex items-center justify-between mb-2" ] [
      div [ _class "text-[24px] font-mono tracking-widest text-neutral-500 uppercase mb-2 pl-1" ] [
        str title
      ]
      div [ _class "h-10 w-36 rounded bg-neutral-300 animate-pulse" ] []
    ]
    table [ _class "w-full min-w-[600px] border-collapse text-left text-sm text-neutral-900 font-mono" ] [
      skeletonTableHeader columnCount
      tbody [ _class "divide-y divide-black border-x border-b border-black" ] [
        for _ in 1 .. rowCount do
          skeletonTableRow columnCount
      ]
    ]
  ]
