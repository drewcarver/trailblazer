module HikePlanner.Views.Components.Autosuggest

open Giraffe.ViewEngine
open HikePlanner.Core

let trailblazerAutosuggest (id: string) (name: string) (labelText: string) (required: Required) endpoint attrs =
    let requiredAttrs =
        match required with
        | Required -> [ attr "required" "required" ]
        | Optional -> []

    let inputAttrs =
        [
            _id id
            _name name
            _class "w-full border border-gray-300 rounded-md shadow-sm focus:ring focus:ring-opacity-50 focus:ring-indigo-500 focus:border-indigo-500"
            attr "hx-get" endpoint
            attr "hx-trigger" "keyup changed delay:500ms"
            attr "list" (id + "-list")
        ]
        @ requiredAttrs
        @ (attrs |> Seq.map (fun (key, value) -> attr key value) |> Seq.toList)

    div [ _class "mb-4" ] [
        label [ _for id; _class "block text-sm font-medium text-gray-700 mb-1" ] [ str labelText ]
        input inputAttrs
    ]