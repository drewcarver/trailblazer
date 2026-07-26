namespace HikePlanner.Core
open Microsoft.AspNetCore.Http
open HikePlanner.Infrastructure
open Giraffe

type ConnectionString = ConnectionString of string

type TrailblazerError =
    | DatabaseError of string
    | FormValidationError of string
    | NotFound of string

type AppEnv = {
    ConnectionString: ConnectionString
}

type EnvironmentWithContext = {
    Environment: AppEnv
    Context: HttpContext
}

type Required =
    | Required
    | Optional

type UserProfile = {
    Id: string
    Email: string
    Name: string
    Picture: string option
}

type Friend = {
    Email     : string
    Name      : string
    Picture   : string option
}

type User = {
    Email     : string
    Name      : string
    Picture   : string option
    Friends   : Friend list
}

type TrailblazerEndpoint = App<EnvironmentWithContext, HttpHandler, HttpHandler>