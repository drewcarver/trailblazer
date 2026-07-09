module HikePlanner.Views.Components.Select

open Giraffe.ViewEngine

type SelectOption = {
    Value: string
    Label: string
    Attributes: (string * string) seq
}

let trailblazerSelect (id: string) (name: string) (labelText: string) options (hyperscript: string option) attrs =
    div [ _class "mb-4" ] [
        label [ _for id; _class "block text-sm font-medium text-gray-700 mb-1" ] [ str labelText ]
        select [ 
            _id id; 
            _name name; 
            _class "w-full border border-gray-300 rounded-md shadow-sm focus:ring focus:ring-opacity-50 focus:ring-indigo-500 focus:border-indigo-500" 
            attr "_" (hyperscript |> Option.defaultValue "")
            yield! attrs |> Seq.map (fun (key, value) -> attr key value)
        ] [
            for opt in options do
                option [ _value opt.Value; yield! opt.Attributes |> Seq.map(fun (key, value) -> attr key value) ] [ str opt.Label ]
        ]
    ]