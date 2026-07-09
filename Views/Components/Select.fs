module HikePlanner.Views.Components.Select

open Giraffe.ViewEngine

type SelectOption = {
    Value: string
    Label: string
}

let trailblazerSelect (id: string) (name: string) (labelText: string) (options) =
    div [ _class "mb-4" ] [
        label [ _for id; _class "block text-sm font-medium text-gray-700 mb-1" ] [ str labelText ]
        select [ _id id; _name name; _class "w-full border border-gray-300 rounded-md shadow-sm focus:ring focus:ring-opacity-50 focus:ring-indigo-500 focus:border-indigo-500" ] [
            for opt in options do
                option [ _value opt.Value ] [ str opt.Label ]
        ]
    ]