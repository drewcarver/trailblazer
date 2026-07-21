open System
open System.Security.Claims
open Microsoft.AspNetCore.Authentication
open Microsoft.AspNetCore.Authentication.Cookies
open Microsoft.AspNetCore.Authentication.Google
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open HikePlanner.Views.Home
open Giraffe
open Giraffe.EndpointRouting
open HikePlanner.Infrastructure
open HikePlanner.Core
open HikePlanner.Handlers.Handlers
open Microsoft.AspNetCore.StaticFiles

let private resolveConnectionString () =
    match Environment.GetEnvironmentVariable("DB_CONNECTION_STRING") with
    | cs when not (String.IsNullOrWhiteSpace cs) -> ConnectionString cs
    | _ -> ConnectionString "Data Source=hikes.db"

let withAppHandler (appEnv: 'env) app next ctx =  
        task {
            let environment = { Environment = appEnv; Context = ctx}
            let! result = App.run environment app

            return! match result with
                    | Ok handler -> handler next ctx
                    | Error handler -> handler next ctx
        }

let loginHandler : HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let properties = AuthenticationProperties(RedirectUri = "/account")
            do! ctx.ChallengeAsync(GoogleDefaults.AuthenticationScheme, properties)
            return! next ctx
        }

let logoutHandler : HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            do! ctx.SignOutAsync()
            do! ctx.SignOutAsync CookieAuthenticationDefaults.AuthenticationScheme
            ctx.User <- ClaimsPrincipal(ClaimsIdentity())
            return! redirectTo false "/" next ctx
        }

let requireLogin : HttpHandler =
    requiresAuthentication (redirectTo false "/login")

let endpoints env = 
    [
        GET [
            route "/" homeHandler
            route "/login" loginHandler
            route "/logout" logoutHandler
            route "/account" (requireLogin >=> withAppHandler env accountHandler)
            route "/hikes/create" (requireLogin >=> withAppHandler env planHandler)
            route "/hikes" (requireLogin >=> withAppHandler env listPlansHandler)
            routef "/hikes/%d:id" (fun id -> requireLogin >=> withAppHandler env (viewHikeHandler id))
        ]
        POST [
            route "/hikes" (requireLogin >=> withAppHandler env saveHikePlan)
            routef "/hikes/%d:id" (fun id -> requireLogin >=> withAppHandler env (updateHikePlan id))
        ]
    ]

[<EntryPoint>]
let main _ =
    let builder = WebApplication.CreateBuilder()

    builder.Services
        .AddAuthentication(fun options ->
            options.DefaultScheme <- CookieAuthenticationDefaults.AuthenticationScheme
            options.DefaultChallengeScheme <- GoogleDefaults.AuthenticationScheme)
        .AddCookie()
        .AddGoogle(fun options ->
            options.ClientId <- builder.Configuration.["Google:ClientId"]
            options.ClientSecret <- builder.Configuration.["Google:ClientSecret"]
            options.ClaimActions.MapJsonKey("urn:google:picture", "picture")
        )
        |> ignore

    builder.Services.AddAuthorization() |> ignore

    let app = builder.Build()

    let provider = FileExtensionContentTypeProvider()
    provider.Mappings.["._hs"] <- "text/hyperscript"
    let staticFileOptions = StaticFileOptions(ContentTypeProvider = provider)
    app.UseStaticFiles staticFileOptions |> ignore

    let env = {
        ConnectionString = resolveConnectionString ()
    }

    app.UseHttpsRedirection()
       .UseRouting()
       .UseAuthentication()
       .UseAuthorization()
       .UseEndpoints(fun e->
            e.MapGiraffeEndpoints (endpoints env)
        ) |> ignore

    app.Run()

    0
