module HikePlanner.Views.Components.Card

open Giraffe.ViewEngine

let tbCard (customClasses: string) (children: XmlNode list) =
    div [ _class ("border-2 border-black rounded-lg p-8 bg-white " + customClasses) ] children

