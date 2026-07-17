namespace HikePlanner.Core

open System

module Utils =
    open HikePlanner.Infrastructure
    open Microsoft.AspNetCore.Http
    open Giraffe

    let getFormHelper<'env, 'T> (ctx: HttpContext) : App<'env, TrailblazerError, 'T> =
        app {
            let! result = 
                ctx.TryBindFormAsync<'T>() 

            return result
        } |> App.mapError (fun e -> FormValidationError e)

    let tryParseDate (input: string) =
        match DateTime.TryParse input with
        | true, parsed -> Ok parsed
        | false, _ -> Error "Invalid date"

    let collapse (result: Result<'a, 'a>): 'a =
        match result with
        | Ok v | Error v -> v

    let always x _ = x

    let tryParseInt (str: string) =
        match System.Int32.TryParse str with
        | true, value -> Ok value
        | false, _    -> Error $"Could not parse '{str}' as an integer"