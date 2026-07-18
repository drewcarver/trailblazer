module HikePlanner.Views.Components.DatePicker

open Giraffe.ViewEngine
open System
open HikePlanner.Core

let datePicker (id: string) (name: string) (labelText: string) (min: DateTime option) (required: Required) (attrs) =
    div [ _class "mb-4" ] [
        label [ _for id; _class "block text-sm font-medium mb-1" ] [ str labelText ]
        input [ 
            _type "date";
            _id id; 
            _name name; 
            match required with
            | Required -> attr "required" "required"
            | Optional -> ()
            _class "w-full border border-gray-300 rounded-lg px-3 py-2 focus:outline-none focus:ring-2 focus:ring-[#4A7043]" 
            _min (match min with | Some m -> m.ToString "yyyy-MM-dd" | None -> "")
            yield! attrs |> List.map (fun (key, value) -> attr key value)
        ]
    ]
