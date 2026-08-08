module HikePlanner.Views.Components.TextInput

open Giraffe.ViewEngine
open HikePlanner.Core

let textInput (id: string) (name: string) (labelText: string) (autofocus: bool) (required: Required) =
    div [ _class "field" ] [
        label [ _for id; _class "field__label" ] [ str labelText ]
        input [ 
            _type "text"; 
            _id id; 
            _name name; 
            match required with
            | Required -> attr "required" "required"
            | Optional -> ()
            _class "field__control" 
        ] 
        span [ _class "field__message" ] [ 
            str "Please enter a value." 
        ]
    ]