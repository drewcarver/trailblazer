module HikePlanner.Views.Components.Heading

open Giraffe.ViewEngine

let h1Elem = h1
let h2Elem = h2
let h3Elem = h3

let tbH1 (text: string) =
    h1Elem [ _class "text-4xl font-bold uppercase tracking-wide" ] [ str text ]

let tbH2 (text: string) =
    h2Elem [ _class "text-2xl font-bold uppercase tracking-wide" ] [ str text ]

let tbH3 (text: string) =
    h3Elem [ _class "text-lg font-bold uppercase" ] [ str text ]

let tbH2' (customClasses: string) (text: string) =
    h2Elem [ _class ("text-2xl font-bold uppercase tracking-wide " + customClasses) ] [ str text ]

