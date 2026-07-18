module HikePlanner.Views.Components.TextInput

open Giraffe.ViewEngine
open HikePlanner.Core

let textInput (id: string) (name: string) (labelText: string) (autofocus: bool) (required: Required) =
    div [ _class "mb-4" ] [
        label [ _for id; _class "block text-sm font-medium mb-1" ] [ str labelText ]
        input [ 
            _type "text"; 
            _id id; 
            _name name; 
            match required with
            | Required -> attr "required" "required"
            | Optional -> ()
            if autofocus then _autofocus; 
            _class "w-full border border-gray-300 rounded-lg px-3 py-2 focus:outline-none focus:ring-2 focus:ring-[#4A7043]" 
        ] 
    ]