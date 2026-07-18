module HikePlanner.Views.Components.LinkButton

open Giraffe.ViewEngine

let trailblazerLinkButton href text additionalClasses =
    let baseClasses =
        "px-4 py-2 text-sm mb-4 inline-flex items-center justify-center px-3 py-1 text-xs font-mono font-bold uppercase border border-black bg-neutral-100 hover:bg-black hover:text-white shadow-[2px_2px_0px_0px_rgba(0,0,0,1)] active:translate-x-[1px] active:translate-y-[1px] active:shadow-[1px_1px_0px_0px_rgba(0,0,0,1)] transition-all cursor-pointer"

    let classes =
        match additionalClasses with
        | Some value when value <> "" -> baseClasses + " " + value
        | _ -> baseClasses

    a [ _href href; _class classes ] [ str text ]
