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
open System.Threading.Tasks

let private resolveConnectionString connectionString authToken =
    match connectionString, authToken with
        | connStr, Some authToken -> "Data Source=" + connStr + ";Auth Token=" + authToken
        | connStr, None -> "Data Source=" + connStr

let withAppHandler appEnv app next ctx =  
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
            route "/" (withAppHandler env homeHandler)
            route "/login" loginHandler
            route "/logout" logoutHandler
            route "/account" (requireLogin >=> withAppHandler env accountHandler)
            route "/hikes/create" (requireLogin >=> withAppHandler env createHikeHandler)
            route "/hikes" (requireLogin >=> withAppHandler env listHikesHandler)
            routef "/hikes/%d:id" (fun id -> requireLogin >=> withAppHandler env (viewHikeHandler id))
        ]
        POST [
            route "/hikes" (requireLogin >=> withAppHandler env saveHikeHandler)
            routef "/hikes/%d:id" (fun id -> requireLogin >=> withAppHandler env (updateHikeHandler id))
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

            options.Events.OnRedirectToAuthorizationEndpoint <- fun context ->
                if context.RedirectUri.StartsWith("http://", StringComparison.OrdinalIgnoreCase) then
                    context.RedirectUri <- context.RedirectUri.Replace("http://", "https://", StringComparison.OrdinalIgnoreCase)
                context.Response.Redirect context.RedirectUri
                Task.CompletedTask
        )
        |> ignore

    builder.Services.AddAuthorization() |> ignore

    let app = builder.Build()

    let forwardedHeadersOptions = ForwardedHeadersOptions()
    forwardedHeadersOptions.ForwardedHeaders <- Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor ||| Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto
    app.UseForwardedHeaders forwardedHeadersOptions |> ignore

    let provider = FileExtensionContentTypeProvider()
    provider.Mappings.["._hs"] <- "text/hyperscript"
    let staticFileOptions = StaticFileOptions(ContentTypeProvider = provider)
    app.UseStaticFiles staticFileOptions |> ignore

    let authToken = app.Configuration.["Turso:AuthToken"] |> Option.ofObj 
    let connectionString = app.Configuration.["Turso:ConnectionString"] 

    let env = {
        ConnectionString = resolveConnectionString connectionString authToken |> ConnectionString  
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
