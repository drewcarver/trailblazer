module HikePlanner.Views.Components.Icon

open Giraffe.ViewEngine

let tbIcon (emoji: string) =
    div [ _class "text-4xl mb-3" ] [ str emoji ]
