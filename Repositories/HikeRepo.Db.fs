namespace HikePlanner.Repositories

module HikeRepoDb =
    open System
    open Turso.Data.Sqlite
    open HikePlanner.Core
    open HikePlanner.Infrastructure

    let ensureUsersTable (conn: SqliteConnection) =
        use cmd = conn.CreateCommand()
        cmd.CommandText <-
            """
CREATE TABLE IF NOT EXISTS user (
    email TEXT PRIMARY KEY,
    details TEXT NOT NULL
);
"""
        cmd.ExecuteNonQuery() |> ignore

    let ensureHikesTable (conn: SqliteConnection) =
        use cmd = conn.CreateCommand()
        cmd.CommandText <-
            """
CREATE TABLE IF NOT EXISTS hike (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    details TEXT NOT NULL
);
"""
        cmd.ExecuteNonQuery() |> ignore

    let toInt64 (value: obj) =
        if isNull value then
            -1L
        else
            match value with
            | :? int64 as id -> id
            | :? int32 as i -> int64 i
            | :? int16 as i -> int64 i
            | :? string as s -> Int64.Parse s
            | _ -> -1L

    let withReader (command: SqliteCommand) (f: SqliteDataReader -> 't) =
        app {
            try
                use sqliteReader = command.ExecuteReader()
                let canRead = sqliteReader.Read()

                if canRead then
                    return f sqliteReader
                else
                    return! App.fail (NotFound "No rows found.")
            with _ ->
                return! App.fail (DatabaseError "Couldn't read from database.")
        }
