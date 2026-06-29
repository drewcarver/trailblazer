module HikePlanner.Repositories.HikeRepo

open System
open Microsoft.Data.Sqlite

type Hike = { 
    Id: int64
    Trail: string
    StartDate: DateTime
    EndDate: DateTime option }

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

let saveHike (connectionString: string) (trail: string) (start: DateTime) (endDate: DateTime option) : int64 =
    use conn = new SqliteConnection(connectionString)
    conn.Open()
    ensureTable conn
    use cmd = conn.CreateCommand()
    cmd.CommandText <- "INSERT INTO hike (trail, start_date, end_date) VALUES ($trail, $start_date, $end_date); SELECT last_insert_rowid();"
    cmd.Parameters.AddWithValue("$trail", trail) |> ignore
    cmd.Parameters.AddWithValue("$start_date", start.ToString("o")) |> ignore
    match endDate with
    | Some d -> cmd.Parameters.AddWithValue("$end_date", d.ToString("o")) |> ignore
    | None -> cmd.Parameters.AddWithValue("$end_date", DBNull.Value) |> ignore
    let result = cmd.ExecuteScalar()
    toInt64 result

let getSavedHikes (connectionString: string) : Hike list =
    use conn = new SqliteConnection(connectionString)
    conn.Open()
    ensureTable conn
    use cmd = conn.CreateCommand()
    cmd.CommandText <- "SELECT id, trail, start_date, end_date FROM hike ORDER BY id;"

    use reader = cmd.ExecuteReader()
    [ while reader.Read() do
          let id = toInt64 (reader.GetValue(0))
          let trail = reader.GetString(1)
          let startDate = DateTime.Parse(reader.GetString(2))
          let endDate =
              match reader.IsDBNull(3) with
              | true -> None
              | false -> Some(DateTime.Parse(reader.GetString(3)))

          yield { Id = id; Trail = trail; StartDate = startDate; EndDate = endDate } ]
