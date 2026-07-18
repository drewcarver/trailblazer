module HikePlanner.Views.Components.Button

open Giraffe.ViewEngine
open FSharp.Core

let trailblazerButton id label text buttonType attrs =
    button [
        _id (id |> Option.defaultValue "")
        _type buttonType
        _label label
        _class "inline-flex items-center justify-center px-3 py-1 text-xs font-mono font-bold uppercase border border-black bg-neutral-100 hover:bg-black hover:text-white shadow-[2px_2px_0px_0px_rgba(0,0,0,1)] active:translate-x-[1px] active:translate-y-[1px] active:shadow-[1px_1px_0px_0px_rgba(0,0,0,1)] transition-all cursor-pointer disabled:opacity-50 disabled:pointer-events-none disabled:shadow-none
"
        yield! attrs |> List.map (fun (key, value) -> attr key value)
    ] [ str text ]