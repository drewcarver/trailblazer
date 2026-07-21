module HikePlanner.Views.Components.Select

open Giraffe.ViewEngine
open HikePlanner.Core

type SelectOption = {
    Value: string
    Label: string
    Attributes: (string * string) seq
}

let trailblazerSelectWithSelected (id: string) (name: string) (labelText: string) (required: Required) options (selectedValue: string option) attrs =
    div [ _class "mb-4" ] [
        label [ _for id; _class "block text-sm font-medium text-gray-700 mb-1" ] [ str labelText ]
        select [ 
            _id id; 
            _name name; 
            _class "w-full border border-gray-300 rounded-md shadow-sm focus:ring focus:ring-opacity-50 focus:ring-indigo-500 focus:border-indigo-500" 
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