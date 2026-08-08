module HikePlanner.Views.Components.Button

open Giraffe.ViewEngine
open FSharp.Core

let trailblazerButton id label text buttonType attrs =
    let attributes =
        [
            _id (id |> Option.defaultValue "")
            _type buttonType
            _label label
            _class "btn btn--compact"
        ]
        @ (attrs |> List.map (fun (key, value) -> attr key value))

    button attributes [ str text ]