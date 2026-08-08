module HikePlanner.Views.Components.LinkButton

open Giraffe.ViewEngine

let trailblazerLinkButton href text additionalClasses =
    let baseClasses = "btn"

    let classes =
        match additionalClasses with
        | Some value when value <> "" -> baseClasses + " " + value
        | _ -> baseClasses

    a [ _href href; _class classes ] [ str text ]
