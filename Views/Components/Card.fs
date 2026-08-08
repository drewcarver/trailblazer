module HikePlanner.Views.Components.Card

open Giraffe.ViewEngine

let tbCard (customClasses: string) (children: XmlNode list) =
    div [ _class ("card " + customClasses) ] children

