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
            _class "peer w-full border border-gray-300 rounded-lg px-3 py-2 focus:outline-none focus:ring-2 focus:ring-[#4A7043]" 
        ] 
        span [ _class "invisible opacity-0 peer-user-invalid:visible peer-user-invalid:opacity-100 transition-all duration-300 ease-in-out block text-xs text-red-500 mt-1" ] [ 
            str "Please enter a value." 
        ]
    ]