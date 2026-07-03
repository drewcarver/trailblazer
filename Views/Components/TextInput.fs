module HikePlanner.Views.Components.TextInput

open Giraffe.ViewEngine

let textInput (id: string) (name: string) (labelText: string) =
    div [ _class "mb-4" ] [
        label [ _for id; _class "block text-sm font-medium mb-1" ] [ str labelText ]
        input [ _type "text"; _id id; _name name; _class "w-full border border-gray-300 rounded-lg px-3 py-2 focus:outline-none focus:ring-2 focus:ring-[#4A7043]" ] 
    ]