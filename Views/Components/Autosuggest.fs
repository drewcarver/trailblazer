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
            _type "text"
            _class "field__control"
            attr "list" (id + "-list")
            attr "data-field-name" name
            attr "data-list-id" (id + "-list")
            attr "data-badges-id" (id + "-badges")
            attr "_" "install Autosuggest"
        ]
        @ requiredAttrs
        @ (attrs |> Seq.map (fun (key, value) -> attr key value) |> Seq.toList)

    div [ _class "field autosuggest__container" ] [
        label [ _for id; _class "field__label" ] [ str labelText ]
        input [ _type "hidden"; _name name; _value "" ]
        input inputAttrs
        div [ _id (id + "-badges"); _class "autosuggest__badges" ] []
        datalist [ _id (id + "-list"); _class "autosuggest__list" ] (options |> List.map datalistOption)
    ]