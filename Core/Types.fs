namespace HikePlanner.Core
open Microsoft.AspNetCore.Http

type ConnectionString = ConnectionString of string


type TrailblazerError =
    | DatabaseError of string
    | NotFound of string


type AppEnv = {
    ConnectionString: ConnectionString
}

type EnvironmentWithContext = {
    Environment: AppEnv
    Context: HttpContext
}