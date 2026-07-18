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

type EnvironmentWithContext<'env> = {
    Environment: 'env
    Context: HttpContext
}

type Required =
    | Required
    | Optional

type UserProfile = {
    Name: string
    Picture: string option
}

type TrailblazerEndpoint<'env> = App<'env, HttpHandler, HttpHandler>