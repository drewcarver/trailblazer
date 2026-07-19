module HikePlanner.Repositories.HikeRepo

open System
open Microsoft.Data.Sqlite
open HikePlanner.Infrastructure
open HikePlanner.Core
open HikePlanner.Core.Utils
open System.Text.Json

type Hike = { 
    Id           : int64
    Trail        : string
    StartDate    : DateTime
    CampPoints   : int64 list 
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
    CampPoints   : TrailPointOfInterest list
}

let private ensureHikersTable (conn: SqliteConnection) =
    use cmd = conn.CreateCommand()
    cmd.CommandText <- """
CREATE TABLE IF NOT EXISTS user (
    email TEXT PRIMARY KEY,
    details TEXT NOT NULL
);
"""
    cmd.ExecuteNonQuery() |> ignore 

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

let private toHikeJson (trail: string) (startDate: DateTime) (campPoints: int list) =
    {| Trail = trail
       StartDate = startDate
       campPoints = campPoints |} 
    |> System.Text.Json.JsonSerializer.Serialize

let saveUser (user: User) =
    app {
        let! { Environment = { ConnectionString = ConnectionString connStr } } = App.ask

        use conn = new SqliteConnection(connStr)
        let! _ = conn.OpenAsync() 
        ensureHikersTable conn

        use cmd = conn.CreateCommand()
        cmd.CommandText <- "INSERT OR REPLACE INTO user (email, details) VALUES ($email, $details);"
        cmd.Parameters.AddWithValue("$email", user.Email) |> ignore
        cmd.Parameters.AddWithValue("$details", System.Text.Json.JsonSerializer.Serialize user) |> ignore

        try
            let rowsAffected = cmd.ExecuteNonQuery()
            if rowsAffected > 0 then
                return! App.succeed ()
            else
                return! App.fail (DatabaseError "No rows were affected when saving the user.")
        with ex -> return! App.fail (DatabaseError (sprintf "Error saving user: %s" ex.Message))
    }

let saveHike (trail: string) (startDate: DateTime) (campPoints: int list)=
    app {
        let! ConnectionString connStr = App.asks(fun env -> env.Environment.ConnectionString) 

        use conn = new SqliteConnection(connStr)
        let! _ = conn.OpenAsync() 
        ensureTable conn

        let hikeDetails = toHikeJson trail startDate campPoints

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

let withPoints (hike: Hike) = 
    app {
        let! points = hike.CampPoints |> List.map getTrailPointOfInterestById

        return {
            Id = hike.Id; 
            Trail = hike.Trail; 
            StartDate = hike.StartDate; 
            CampPoints = points
        }
    } |> App.mapError (DatabaseError "Couldn't map trail points of interest." |> always)

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
            let options = JsonSerializerOptions(PropertyNameCaseInsensitive = true)
            let parsed = System.Text.Json.JsonSerializer.Deserialize<Hike>(details, options)
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
            let options = JsonSerializerOptions(PropertyNameCaseInsensitive = true)
            let parsed = System.Text.Json.JsonSerializer.Deserialize<Hike>(details, options)
            let hike = { parsed with Id = id } 
            
            if rdr.Read() then readAllHikes (hike::hikes) rdr else hike::hikes

        let! hikes = withReader cmd (readAllHikes [])

        return! hikes |> List.map withPoints
    }

let getUser userName =
    app {
        let! { Environment = { ConnectionString = ConnectionString connStr } } = App.ask
        
        use conn = new SqliteConnection(connStr)
        conn.Open() |> ignore
        ensureHikersTable conn

        use cmd = conn.CreateCommand()
        cmd.CommandText <- 
            """
            WITH main_hiker AS (
                SELECT email, details 
                FROM hiker 
                WHERE email = $email 
                LIMIT 1
            ),
            friend_details AS (
                SELECT 
                    json_object(
                        'email', f.email,
                        'firstName', json_extract(f.details, '$.firstName'),
                        'lastName', json_extract(f.details, '$.lastName'),
                        'picture', json_extract(f.details, '$.picture')
                    ) AS friend_obj
                FROM main_hiker m
                CROSS JOIN json_each(m.details, '$.friends') je
                LEFT JOIN hiker f ON f.email = json_extract(je.value, '$.email')
            )
            SELECT 
                m.email,
                json_set(m.details, '$.friends', (
                    SELECT json_group_array(json(friend_obj)) 
                    FROM friend_details 
                    WHERE friend_obj IS NOT NULL
                )) AS enriched_details
            FROM main_hiker m;
            """
        cmd.Parameters.AddWithValue("$email", userName) |> ignore

        let! hiker = withReader cmd (fun rdr ->
            let email = rdr.GetString 0
            let hikerJson = rdr.GetString 1
            let options = JsonSerializerOptions(PropertyNameCaseInsensitive = true)
            let hiker = System.Text.Json.JsonSerializer.Deserialize<User>(hikerJson, options)
            { hiker with Email = email }
        )

        return hiker
    }