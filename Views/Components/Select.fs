module HikePlanner.Views.Components.Select

open Giraffe.ViewEngine
open HikePlanner.Core

type SelectOption = {
    Value: string
    Label: string
    Attributes: (string * string) seq
}

let trailblazerSelectWithSelected (id: string) (name: string) (labelText: string) (required: Required) options (selectedValue: string option) attrs =
    div [ _class "field" ] [
        label [ _for id; _class "field__label" ] [ str labelText ]
        select [ 
            _id id; 
            _name name; 
            _class "field__control" 
            match required with
            | Required -> attr "required" "required"
            | Optional -> ()
            yield! attrs |> Seq.map (fun (key, value) -> attr key value)
        ] [
            option [ 
                _value ""
                attr "disabled" "disabled"
                if selectedValue.IsNone then
                    attr "selected" "selected"
            ] [ str "-- Select an option --" ]
            for opt in options do
                option [ 
                    _value opt.Value
                    if selectedValue = Some opt.Value then
                        attr "selected" "selected"
                    yield! opt.Attributes |> Seq.map(fun (key, value) -> attr key value) 
                ] [ str opt.Label ]
        ]
    ]

let trailblazerSelect (id: string) (name: string) (labelText: string) (required: Required) options attrs =
    trailblazerSelectWithSelected id name labelText required options None attrs