module HikePlanner.Views.Components.Button

open Giraffe.ViewEngine
open FSharp.Core

let trailblazerButton label text buttonType hypertext =
    button [
        _type buttonType
        _label label
        _class "inline-flex items-center justify-center px-3 py-1 text-xs font-mono font-bold uppercase border border-black bg-neutral-100 hover:bg-black hover:text-white shadow-[2px_2px_0px_0px_rgba(0,0,0,1)] active:translate-x-[1px] active:translate-y-[1px] active:shadow-[1px_1px_0px_0px_rgba(0,0,0,1)] transition-all cursor-pointer"
        attr "_" (hypertext |> Option.defaultValue "")
    ] [ str text ]