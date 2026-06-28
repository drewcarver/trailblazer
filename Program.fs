open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open HikePlanner.Views
open Giraffe

// let homeHandler =
//     htmlString """
// <!DOCTYPE html>
// <html>
// <head>
//     <title>HTMX + Giraffe</title>
//     <script src="https://unpkg.com/htmx.org@2.0.7"></script>
// </head>
// <body>
//     <h1>HTMX + Giraffe</h1>

//     <button hx-get="/hello"
//             hx-target="#result"
//             hx-swap="innerHTML">
//         Click Me
//     </button>

//     <div id="result"></div>
// </body>
// </html>
// """

let helloHandler =
    htmlString """
<div>
    <strong>Hello from Giraffe!</strong>
    <p>This content was loaded with HTMX.</p>
</div>
"""

let webApp =
    choose [
        GET >=>
            choose [
                route "/" >=> homeHandler
                route "/hello" >=> helloHandler
            ]
    ]

[<EntryPoint>]
let main args =
    let builder = WebApplication.CreateBuilder(args)

    builder.Services.AddGiraffe() |> ignore

    let app = builder.Build()

    app.UseGiraffe webApp

    app.Run()

    0