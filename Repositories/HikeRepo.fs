module HikePlanner.Repositories.HikeRepo

open System
open Microsoft.Data.Sqlite
open HikePlanner.Infrastructure
open System.Threading.Tasks
open HikePlanner.Core

type Hike = { 
    Id: int64
    Trail: string
    StartDate: DateTime
    EndDate: DateTime }

type TrailPointOfInterest = { 
    Id: int64
    TrailName: string
    TrailMile: float
    Name: string
    }

let private ensureTable (conn: SqliteConnection) =
    use cmd = conn.CreateCommand()
    cmd.CommandText <- """
CREATE TABLE IF NOT EXISTS hike (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    trail TEXT NOT NULL,
    start_date TEXT NOT NULL,
    end_date TEXT
);"""
    cmd.ExecuteNonQuery() |> ignore

let private toInt64 (value: obj) =
    if isNull value then
        -1L
    else
        match value with
        | :? int64 as id -> id
        | :? int32 as i -> int64 i
        | :? int16 as i -> int64 i
        | :? string as s -> Int64.Parse s
        | _ -> -1L

let saveHike (trail: string) (start: DateTime) (endDate: DateTime) =
    app {
        let! { Environment = { ConnectionString = ConnectionString connStr }} = App.ask

        use conn = new SqliteConnection(connStr)
        conn.Open() |> ignore
        ensureTable conn

        use cmd = conn.CreateCommand()
        cmd.CommandText <- "INSERT INTO hike (trail, start_date, end_date) VALUES ($trail, $start_date, $end_date); SELECT last_insert_rowid();"
        cmd.Parameters.AddWithValue("$trail", trail) |> ignore
        cmd.Parameters.AddWithValue("$start_date", start.ToString "o") |> ignore
        cmd.Parameters.AddWithValue("$end_date", endDate.ToString "o") |> ignore

        let! result = 
            App.catch 
                (fun ex -> DatabaseError (sprintf "Error saving hike: %s" ex.Message)) 
                (fun () -> cmd.ExecuteScalar() |> toInt64 |> Task.FromResult)

        return toInt64 result
    }

let getSavedHikes =
    app {
        let! { Environment = { ConnectionString = ConnectionString connStr } } = App.ask
        
        use! conn = 
          try
              let connection = new SqliteConnection(connStr)
              connection.Open() |> ignore
              ensureTable connection
              App.succeed connection
          with ex ->
              App.fail (DatabaseError "Couldn't open database")

        use cmd = conn.CreateCommand()
        cmd.CommandText <- "SELECT id, trail, start_date, end_date FROM hike ORDER BY id;"

        return! try
                use rdr = cmd.ExecuteReader()
                let results =
                    [ while rdr.Read() do
                        let id = toInt64 (rdr.GetValue 0)
                        let trail = rdr.GetString 1
                        let startDate = DateTime.Parse(rdr.GetString 2)
                        let endDate = DateTime.Parse(rdr.GetString 3)

                        yield { Id = id; Trail = trail; StartDate = startDate; EndDate = endDate } ]
                App.succeed results 
        with ex ->
            App.fail (DatabaseError (sprintf "Error retrieving hikes: %s" ex.Message)) 
    }

let withReader (command: SqliteCommand) f : App<'a, TrailblazerError, 'b> =
    try
        use sqliteReader = command.ExecuteReader()
        if sqliteReader.Read() then
            f sqliteReader |> App.succeed
        else
            App.fail (NotFound "No rows found.")
    with ex ->
        App.fail (DatabaseError (sprintf "Error reading from SQLite: %s" ex.Message))

let getHikeById (id: int64) =
    app {
        let! { Environment = { ConnectionString = ConnectionString connStr } } = App.ask

        use conn = new SqliteConnection(connStr)
        conn.Open() |> ignore
        ensureTable conn

        use cmd = conn.CreateCommand()
        cmd.CommandText <- "SELECT id, trail, start_date, end_date FROM hike WHERE id = $id LIMIT 1;"
        cmd.Parameters.AddWithValue("$id", id) |> ignore

        return! withReader cmd (fun rdr ->
            let id = toInt64 (rdr.GetValue 0)
            let trail = rdr.GetString 1
            let startDate = DateTime.Parse(rdr.GetString 2)
            let endDate = DateTime.Parse(rdr.GetString 3)

            { Id = id; Trail = trail; StartDate = startDate; EndDate = endDate }
        )
    }

let getHikeByName (trailName: string) : App<ConnectionString, TrailblazerError, Hike> =
    app {
        let! ConnectionString connStr = App.ask

        use conn = new SqliteConnection(connStr)
        conn.Open() |> ignore
        ensureTable conn

        use cmd = conn.CreateCommand()
        cmd.CommandText <- "SELECT id, trail, start_date, end_date FROM hike WHERE trail = $trail LIMIT 1;"
        cmd.Parameters.AddWithValue("$trail", trailName) |> ignore

        return! withReader cmd (fun rdr ->
            let id = toInt64 (rdr.GetValue 0)
            let trail = rdr.GetString 1
            let startDate = DateTime.Parse(rdr.GetString 2)
            let endDate = DateTime.Parse(rdr.GetString 3)

            { Id = id; Trail = trail; StartDate = startDate; EndDate = endDate }
        )
    }

let getTrailPointsOfInterest (trailName: string) =
    app {
        let! { Environment = { ConnectionString = ConnectionString connStr } } = App.ask

        use conn = new SqliteConnection(connStr)
        conn.Open() |> ignore

        use cmd = conn.CreateCommand()
        cmd.CommandText <- "SELECT Id, TrailName, TrailMile, Name FROM TrailPointsOfInterest WHERE TrailName = $trailName ORDER BY TrailMile;"
        cmd.Parameters.AddWithValue("$trailName", trailName) |> ignore

        return! App.catch
            (fun ex -> DatabaseError (sprintf "Error retrieving points of interest: %s" ex.Message))
            (fun _ -> 
                use rdr = cmd.ExecuteReader()

                [ while rdr.Read() do
                    let id = toInt64 (rdr.GetValue 0)
                    let trailName = rdr.GetString 1
                    let trailMile = rdr.GetDouble 2
                    let name = rdr.GetString 3

                    yield { Id = id; TrailName = trailName; TrailMile = trailMile; Name = name } ]
            )
    }