module HikePlanner.Views.Components.Table

open Giraffe.ViewEngine
open FSharp.Core
open Button
open LinkButton

let trailblazerTableButton label text = 
  td [ _class "px-4 py-3 text-center whitespace-nowrap" ] [
    trailblazerButton None label text "Button" []
  ]

type ColumnValue =
    | StringValue of string
    | XmlNodeValue of XmlNode
let trailblazerTableColumn (value: ColumnValue) =
    td [ _class "h-[3rem] px-4 py-3 border-r border-black font-sans font-medium truncate max-w-[200px]" ] [ match value with | StringValue s -> str s | XmlNodeValue n -> n ]

let trailblazerTableRow cols = 
  tr [ _class "hover:bg-neutral-50 transition-colors" ] cols
  

let trailblazerTableHeader headers =
    thead [] [
        tr [ _class "border border-black bg-neutral-100" ] [
            for header in headers do
                th [ _scope "col"; _class "px-4 py-3 font-bold uppercase tracking-wider border-r border-black w-1/5" ] [ str header ]
        ]
    ]

let trailblazerTable title header rows  =
  div [ _class "w-full overflow-x-auto border border-black rounded-lg bg-white p-4 font-sans selection:bg-neutral-200" ] [
        div [ _class "flex items-center justify-between mb-2" ] [
          div [ _class "text-[24px] font-mono tracking-widest text-neutral-500 uppercase mb-2 pl-1" ] [
              str title
          ]
          trailblazerLinkButton "/hikes/create" "Create New Hike" None
        ]
        table [ _class "w-full min-w-[600px] border-collapse text-left text-sm text-neutral-900 font-mono" ] [
          header
          tbody [ _class "divide-y divide-black border-x border-b border-black" ] rows
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
