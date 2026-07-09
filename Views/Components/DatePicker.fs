module HikePlanner.Views.Components.DatePicker

open Giraffe.ViewEngine
open System

let datePicker (id: string) (name: string) (labelText: string) (min: DateTime option) (max: DateTime option) =
    div [ _class "mb-4" ] [
        label [ _for id; _class "block text-sm font-medium mb-1" ] [ str labelText ]
        input [ 
            _type "date";
            _id id; 
            _name name; 
            _class "w-full border border-gray-300 rounded-lg px-3 py-2 focus:outline-none focus:ring-2 focus:ring-[#4A7043]" 
            _min (match min with | Some value -> value.ToString "o" | None -> "")
            _max (match max with | Some value -> value.ToString "o" | None -> "")
        ]
    ]
