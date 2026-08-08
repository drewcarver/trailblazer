module HikePlanner.Views.Components.DatePicker

open Giraffe.ViewEngine
open System
open HikePlanner.Core

let datePicker (id: string) (name: string) (labelText: string) (min: DateTime option) (required: Required) (attrs) =
    div [ _class "field" ] [
        label [ _for id; _class "field__label" ] [ str labelText ]
        input [ 
            _type "date";
            _id id; 
            _name name; 
            match required with
            | Required -> attr "required" "required"
            | Optional -> ()
            _class "field__control" 
            _min (match min with | Some m -> m.ToString "yyyy-MM-dd" | None -> "")
            yield! attrs |> List.map (fun (key, value) -> attr key value)
        ]
    ]
