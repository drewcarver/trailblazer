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

let private defaultConnectionString = ConnectionString "Data Source=hikes.db"

let withAppHandler (appEnv: 'env) app next ctx =  
        task {
            let environment = { Environment = appEnv; Context = ctx}
            let! result = App.run environment app

            return! match result with
                    | Ok handler -> handler next ctx
                    | Error handler -> handler next ctx
        }

// Kick browser to Google sign-in screen
let loginHandler : HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let properties = AuthenticationProperties(RedirectUri = "/account")
            do! ctx.ChallengeAsync(GoogleDefaults.AuthenticationScheme, properties)
            return! next ctx
        }

// Clear cookie to end session
let logoutHandler : HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            do! ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme)
            return! redirectTo false "/" next ctx
        }

let accountHandler : HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        let name = ctx.User.FindFirst(ClaimTypes.Name) |> Option.ofObj |> Option.map (fun c -> c.Value) |> Option.defaultValue "hiker"
        text (sprintf "Welcome, %s!" name) next ctx

// Gate any handler behind login; unauthenticated visitors get redirected to /login
let requireLogin : HttpHandler =
    requiresAuthentication (redirectTo false "/login")

let endpoints env = 
    [
        GET [
            route "/" homeHandler
            route "/login" loginHandler
            route "/logout" logoutHandler
            route "/account" (requireLogin >=> accountHandler)
            route "/plan/create" (requireLogin >=> withAppHandler env planHandler)
            route "/plan" (requireLogin >=> withAppHandler env listPlansHandler)
            routef "/plan/%d:id" (fun id -> requireLogin >=> withAppHandler env (viewHikeHandler id))
        ]
        POST [
            route "/plan" (requireLogin >=> withAppHandler env saveHikePlan)
        ]
    ]

[<EntryPoint>]
let main _ =
    let builder = WebApplication.CreateBuilder()

    builder.Services
        .AddAuthentication(fun options ->
            options.DefaultScheme <- CookieAuthenticationDefaults.AuthenticationScheme
            options.DefaultChallengeScheme <- GoogleDefaults.AuthenticationScheme)
        .AddCookie(fun options ->
            options.Cookie.Name <- "TrailBlazerAuthCookie"
            options.Cookie.SameSite <- SameSiteMode.Lax
            options.Cookie.SecurePolicy <- CookieSecurePolicy.SameAsRequest)
        .AddGoogle(fun options ->
            options.ClientId <- builder.Configuration.["Google:ClientId"]
            options.ClientSecret <- builder.Configuration.["Google:ClientSecret"]
            options.CorrelationCookie.SameSite <- SameSiteMode.Lax
            options.CorrelationCookie.SecurePolicy <- CookieSecurePolicy.SameAsRequest    
        )
        |> ignore

    builder.Services.AddAuthorization() |> ignore

    let app = builder.Build()

    let provider = FileExtensionContentTypeProvider()
    provider.Mappings.["._hs"] <- "text/hyperscript"
    let staticFileOptions = StaticFileOptions(ContentTypeProvider = provider)
    app.UseStaticFiles staticFileOptions |> ignore

    let env = {
        ConnectionString = defaultConnectionString
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
