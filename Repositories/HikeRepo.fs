module HikePlanner.Repositories.HikeRepo

open System
open Microsoft.Data.Sqlite
open HikePlanner.Infrastructure
open HikePlanner.Core

type Hike = { 
    Id           : int64
    Trail        : string
    StartDate    : DateTime
    EndDate      : DateTime
    StartPointId : int64
    EndPointId   : int64
 }

type TrailPointOfInterest = { 
    Id          : int64
    Name        : string
    TrailName   : string
    TrailMile   : float
    }

type SavedHike = {
    Id           : int64
    Trail        : string
    StartDate    : DateTime
    EndDate      : DateTime
    StartPoint   : TrailPointOfInterest
    EndPoint     : TrailPointOfInterest
}

let private ensureTable (conn: SqliteConnection) =
    use cmd = conn.CreateCommand()
    cmd.CommandText <- """
CREATE TABLE IF NOT EXISTS hike (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    details TEXT NOT NULL
);
"""
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

let private toHikeJson (trail: string) (startDate: DateTime) (endDate: DateTime) (startPointId: int64) (endPointId: int64) =
    {| Trail = trail
       StartDate = startDate
       EndDate = endDate
       StartPointId = startPointId
       EndPointId = endPointId |} 
    |> System.Text.Json.JsonSerializer.Serialize

let saveHike (trail: string) (startDate: DateTime) (endDate: DateTime) (startPointId: int64) (endPointId: int64) =
    app {
        let! ConnectionString connStr = App.asks(fun env -> env.Environment.ConnectionString) 

        use conn = new SqliteConnection(connStr)
        let! _ = conn.OpenAsync() 
        ensureTable conn

        let hikeDetails = toHikeJson trail startDate endDate startPointId endPointId

        use cmd = conn.CreateCommand()
        cmd.CommandText <- "INSERT INTO hike (details) VALUES ($details); SELECT last_insert_rowid();"
        cmd.Parameters.AddWithValue("$details", hikeDetails) |> ignore

        try
            let id = cmd.ExecuteScalar() 
            return! App.succeed id
        with ex -> return! App.fail (DatabaseError (sprintf "Error saving hike: %s" ex.Message))
    }

let withReader (command: SqliteCommand) (f: SqliteDataReader -> 'b) =
    app {
        try
            use sqliteReader = command.ExecuteReader()
            let canRead = sqliteReader.Read()

            if canRead then
                let results = f sqliteReader 
                return! App.succeed results
            else
                return! App.fail (NotFound "No rows found.")
        with _ -> 
            return! App.fail (DatabaseError "Couldn't read from database.")
    }


let getHikeByTrailName (trailName: string) =
    app {
        let! { Environment = { ConnectionString = ConnectionString connStr } } = App.ask

        use conn = new SqliteConnection(connStr)
        conn.Open() |> ignore
        ensureTable conn

        use cmd = conn.CreateCommand()
        cmd.CommandText <- "SELECT id, details FROM hike WHERE trail = $trail LIMIT 1;"
        cmd.Parameters.AddWithValue("$trail", trailName) |> ignore

        return! withReader cmd (fun rdr ->
            let id = toInt64 (rdr.GetValue 0)
            let details = rdr.GetString 1
            let parsed = System.Text.Json.JsonSerializer.Deserialize<Hike>(details)
            { parsed with Id = id }
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

let getTrailPointOfInterestById (id: int64) =
    app {
        let! { Environment = { ConnectionString = ConnectionString connStr } } = App.ask

        use conn = new SqliteConnection(connStr)
        let! _ = conn.OpenAsync() 
        ensureTable conn

        use cmd = conn.CreateCommand()
        cmd.CommandText <- "SELECT Id, TrailName, TrailMile, Name FROM TrailPointsOfInterest WHERE Id = $id ORDER BY TrailMile; LIMIT 1;"
        cmd.Parameters.AddWithValue("$id", id) |> ignore

        return! withReader cmd (fun rdr ->
            let id = toInt64 (rdr.GetValue 0)
            let trailName = rdr.GetString 1
            let trailMile = rdr.GetDouble 2
            let name = rdr.GetString 3

            { Id = id; TrailName = trailName; TrailMile = trailMile; Name = name }
        )
    }

let withPoints hike = 
    app {
        let! startPoint = getTrailPointOfInterestById hike.StartPointId
        and! endPoint   = getTrailPointOfInterestById hike.EndPointId

        return {
            Id = hike.Id; 
            Trail = hike.Trail; 
            StartDate = hike.StartDate; 
            EndDate = hike.EndDate; 
            StartPoint = startPoint; 
            EndPoint = endPoint; 
        }
    }

let getHikeById (id: int64) =
    app {
        let! { Environment = { ConnectionString = ConnectionString connStr } } = App.ask

        use conn = new SqliteConnection(connStr)
        conn.Open() |> ignore
        ensureTable conn

        use cmd = conn.CreateCommand()
        cmd.CommandText <- "SELECT id, details FROM hike WHERE id = $id LIMIT 1;"
        cmd.Parameters.AddWithValue("$id", id) |> ignore

        let! hike = withReader cmd (fun rdr ->
            let id = toInt64 (rdr.GetValue 0)
            let details = rdr.GetString 1
            let parsed = System.Text.Json.JsonSerializer.Deserialize<Hike>(details)
            { parsed with Id = id }
        )

        return! withPoints hike
    } 

let getHikes =
    app {
        let! { Environment = { ConnectionString = ConnectionString connStr } } = App.ask
        
        use conn = new SqliteConnection(connStr)
        conn.Open() |> ignore
        ensureTable conn

        use cmd = conn.CreateCommand()
        cmd.CommandText <- "SELECT id, details FROM hike ORDER BY id;"

        let rec readAllHikes hikes (rdr: SqliteDataReader) =
            let id = toInt64 (rdr.GetValue 0)
            let details = rdr.GetString 1
            let parsed = System.Text.Json.JsonSerializer.Deserialize<Hike>(details)
            let hike = { parsed with Id = id } 
            
            if rdr.Read() then readAllHikes (hike::hikes) rdr else hike::hikes

        let! hikes = withReader cmd (readAllHikes [])
        printfn "Found %d hikes" hikes.Length

        return! hikes |> List.map withPoints
    }
