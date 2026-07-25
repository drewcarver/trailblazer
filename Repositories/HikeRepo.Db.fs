namespace HikePlanner.Repositories

module HikeRepoDb =
    open System
    open HikePlanner.Core
    open HikePlanner.Infrastructure
    open Nelknet.LibSQL.Data
    open System.Data.Common


    let ensureUsersTable (conn: DbConnection) =
        use cmd = conn.CreateCommand()
        cmd.CommandText <-
            """
CREATE TABLE IF NOT EXISTS user (
    email TEXT PRIMARY KEY,
    details TEXT NOT NULL
);
"""
        cmd.ExecuteNonQuery() |> ignore

    let ensureHikesTable (conn: DbConnection) =
        use cmd = conn.CreateCommand()
        cmd.CommandText <-
            """
CREATE TABLE IF NOT EXISTS hike (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    details TEXT NOT NULL
);
"""
        cmd.ExecuteNonQuery() |> ignore

    let openConnection connStr = 
        app {
            let conn = new LibSQLConnection(connStr)
            let! _ = conn.OpenAsync()
            ensureHikesTable conn
            ensureUsersTable conn

            return conn
        }

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

    let withReader (command: DbCommand) (f: DbDataReader -> 't) =
        app {
            try
                use reader = command.ExecuteReader()
                let canRead = reader.Read()

                if canRead then
                    return f reader
                else
                    return! App.fail (NotFound "No rows found.")
            with _ ->
                return! App.fail (DatabaseError "Couldn't read from database.")
        }
