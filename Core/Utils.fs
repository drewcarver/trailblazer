namespace HikePlanner.Core

open System

module Utils =
    let tryParseDate (input: string) =
        match DateTime.TryParse input with
        | true, parsed -> Ok parsed
        | false, _ -> Error "Invalid date"

    let collapse (result: Result<'a, 'a>): 'a =
        match result with
        | Ok v | Error v -> v
