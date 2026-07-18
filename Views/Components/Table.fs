module HikePlanner.Views.Components.Table

open Giraffe.ViewEngine
open FSharp.Core
open Button

let trailblazerTableButton label text = 
  td [ _class "px-4 py-3 text-center whitespace-nowrap" ] [
    trailblazerButton None label text "Button" None
  ]

type ColumnValue =
    | StringValue of string
    | XmlNodeValue of XmlNode
let trailblazerTableColumn (value: ColumnValue) =
    td [ _class "px-4 py-3 border-r border-black font-sans font-medium truncate max-w-[200px]" ] [ match value with | StringValue s -> str s | XmlNodeValue n -> n ]

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
        div [ _class "text-[10px] font-mono tracking-widest text-neutral-500 uppercase mb-2 pl-1" ] [
            str title
        ]
        table [ _class "w-full min-w-[600px] border-collapse text-left text-sm text-neutral-900 font-mono" ] [
          header
          tbody [ _class "divide-y divide-black border-x border-b border-black" ] rows
        ]
  ]
