module StringUtils

let toOption (s: string) =
    if System.String.IsNullOrWhiteSpace(s) then None
    else Some s