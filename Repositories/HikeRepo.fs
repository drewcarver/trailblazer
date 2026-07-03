module HikePlanner.Repositories.HikeRepo

open System
open Microsoft.Data.Sqlite
open HikePlanner
open HikePlanner.App

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

let saveHike (trail: string) (start: DateTime) (endDate: DateTime option) : App<string, exn, int64> =
    App.ask<string, exn>
    |> App.bind (fun connStr ->
        App.ofTask (task {
            use conn = new SqliteConnection(connStr)
            conn.Open() |> ignore
            ensureTable conn
            use cmd = conn.CreateCommand()
            cmd.CommandText <- "INSERT INTO hike (trail, start_date, end_date) VALUES ($trail, $start_date, $end_date); SELECT last_insert_rowid();"
            cmd.Parameters.AddWithValue("$trail", trail) |> ignore
            cmd.Parameters.AddWithValue("$start_date", start.ToString("o")) |> ignore

            match endDate with
            | Some d -> cmd.Parameters.AddWithValue("$end_date", d.ToString("o")) |> ignore
            | None -> cmd.Parameters.AddWithValue("$end_date", DBNull.Value) |> ignore

            let result = cmd.ExecuteScalar()
            return toInt64 result
        }))

let getSavedHikes : App<string, exn, Hike list> =
    App.ask<string, exn>
    |> App.bind (fun connStr ->
        App.ofTask (task {
            use conn = new SqliteConnection(connStr)
            conn.Open() |> ignore
            ensureTable conn
            use cmd = conn.CreateCommand()
            cmd.CommandText <- "SELECT id, trail, start_date, end_date FROM hike ORDER BY id;"

            use rdr = cmd.ExecuteReader()
            let results =
                [ while rdr.Read() do
                      let id = toInt64 (rdr.GetValue(0))
                      let trail = rdr.GetString(1)
                      let startDate = DateTime.Parse(rdr.GetString(2))
                      let endDate =
                          match rdr.IsDBNull(3) with
                          | true -> None
                          | false -> Some(DateTime.Parse(rdr.GetString(3)))

                      yield { Id = id; Trail = trail; StartDate = startDate; EndDate = endDate } ]
            return results
        }))

let getHikeByName (trailName: string) : App<string, exn, Hike option> =
    App.ask<string, exn>
    |> App.bind (fun connStr ->
        App.ofTask (task {
            use conn = new SqliteConnection(connStr)
            conn.Open() |> ignore
            ensureTable conn
            use cmd = conn.CreateCommand()
            cmd.CommandText <- "SELECT id, trail, start_date, end_date FROM hike WHERE trail = $trail LIMIT 1;"
            cmd.Parameters.AddWithValue("$trail", trailName) |> ignore

            use rdr = cmd.ExecuteReader()
            if rdr.Read() then
                let id = toInt64 (rdr.GetValue(0))
                let trail = rdr.GetString(1)
                let startDate = DateTime.Parse(rdr.GetString(2))
                let endDate =
                    match rdr.IsDBNull(3) with
                    | true -> None
                    | false -> Some(DateTime.Parse(rdr.GetString(3)))
                return Some { Id = id; Trail = trail; StartDate = startDate; EndDate = endDate }
            else
                return None
        }))
