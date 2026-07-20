module HikePlanner.Views.Components.Description

open Giraffe.ViewEngine

let tbDescription (text: string) =
    p [ _class "text-base mb-6 leading-relaxed" ] [ str text ]
