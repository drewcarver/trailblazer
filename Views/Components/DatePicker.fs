module HikePlanner.Views.Components.DatePicker

open Giraffe.ViewEngine
open System

let datePicker (id: string) (name: string) (labelText: string) (hyperscript: string option) (min: DateTime option) =
    div [ _class "mb-4" ] [
        label [ _for id; _class "block text-sm font-medium mb-1" ] [ str labelText ]
        input [ 
            _type "date";
            _id id; 
            _name name; 
            _class "w-full border border-gray-300 rounded-lg px-3 py-2 focus:outline-none focus:ring-2 focus:ring-[#4A7043]" 
            _min (match min with | Some m -> m.ToString "yyyy-MM-dd" | None -> "")
            attr "_" (hyperscript |> Option.defaultValue "")
        ]
    ]
