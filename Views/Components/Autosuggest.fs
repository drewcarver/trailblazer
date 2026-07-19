module HikePlanner.Views.Components.Autosuggest

open Giraffe.ViewEngine
open HikePlanner.Core

let private datalistOption (value: string, label: string) =
    option [ _value value ] [ str label ]

let trailblazerAutosuggest (id: string) (name: string) (labelText: string) (required: Required) (options: (string * string) list) attrs =
    let requiredAttrs =
        match required with
        | Required -> [ attr "required" "required" ]
        | Optional -> []

    let inputAttrs =
        [
            _id id
            _name name
            _type "text"
            _class "w-full border border-gray-300 rounded-md shadow-sm focus:ring focus:ring-opacity-50 focus:ring-indigo-500 focus:border-indigo-500"
            attr "list" (id + "-list")
        ]
        @ requiredAttrs
        @ (attrs |> Seq.map (fun (key, value) -> attr key value) |> Seq.toList)

    div [ _class "mb-4" ] [
        label [ _for id; _class "block text-sm font-medium text-gray-700 mb-1" ] [ str labelText ]
        input inputAttrs
        datalist [ _id (id + "-list"); _class "rounded-md border border-gray-300 bg-white text-sm text-gray-700" ] (options |> List.map datalistOption)
    ]