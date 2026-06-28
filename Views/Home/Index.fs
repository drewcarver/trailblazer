module HikePlanner.Views

open Giraffe.ViewEngine
open Giraffe

let indexView =
    html [] [
        head [] [
            script [ _src "https://unpkg.com/htmx.org" ] []
        ]
        body [] [
            h1 [] [ str "HTMX + Giraffe" ]
        ]
    ]

let homeHandler: HttpHandler = 
    htmlView indexView