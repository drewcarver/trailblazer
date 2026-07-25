namespace HikePlanner.Repositories

module HikeRepoHikes =
    open System
    open HikePlanner.Core
    open HikePlanner.Core.Utils
    open HikePlanner.Infrastructure
    open HikePlanner.Repositories.HikeRepoTypes
    open System.Text.Json
    open System.Data.Common

    let private toHikeJson (username: string) (trail: string) (startDate: DateTime) (campPoints: int list) =
        {| username = username
           Trail = trail
           StartDate = startDate
           campPoints = campPoints |}
        |> JsonSerializer.Serialize

    let saveHike (username: string) (trail: string) (startDate: DateTime) (campPoints: int list) =
        app {
            let! ConnectionString connStr = App.asks (fun env -> env.Environment.ConnectionString)

            use! conn = HikeRepoDb.openConnection connStr

            let hikeDetails = toHikeJson username trail startDate campPoints

            use insertCmd = conn.CreateCommand()
            insertCmd.CommandText <- "INSERT INTO hike (details) VALUES ($details);"
            insertCmd.Parameters.AddWithValue("$details", hikeDetails) |> ignore

            use idCmd = conn.CreateCommand()
            idCmd.CommandText <- "SELECT last_insert_rowid();"

            try
                insertCmd.ExecuteNonQuery() |> ignore
                let idObj = idCmd.ExecuteScalar()
                let id = HikeRepoDb.toInt64 idObj

                if id < 0L then
                    return! App.fail (DatabaseError "Error saving hike: failed to retrieve inserted id.")
                else
                    return! App.succeed id
            with ex ->
                return! App.fail (DatabaseError (sprintf "Error saving hike: %s" ex.Message))
        }

    let updateHike (id: int64) (username: string) (trail: string) (startDate: DateTime) (campPoints: int list) =
        app {
            let! ConnectionString connStr = App.asks (fun env -> env.Environment.ConnectionString)

            use! conn = HikeRepoDb.openConnection connStr

            let hikeDetails = toHikeJson username trail startDate campPoints

            use cmd = conn.CreateCommand()
            cmd.CommandText <- "UPDATE hike SET details = $details WHERE id = $id AND json_extract(details, '$.username') = $username;"
            cmd.Parameters.AddWithValue("$details", hikeDetails) |> ignore
            cmd.Parameters.AddWithValue("$id", id) |> ignore
            cmd.Parameters.AddWithValue("$username", username) |> ignore

            try
                let changes = cmd.ExecuteNonQuery()

                if changes = 0 then
                    return! App.fail (NotFound (sprintf "Hike with id %d was not found." id))
                else
                    return! App.succeed id
            with ex ->
                return! App.fail (DatabaseError (sprintf "Error updating hike: %s" ex.Message))
        }

    let private withPoints (hike: Hike) =
        app {
            let! points = hike.CampPoints |> List.map HikeRepoTrailPoints.getTrailPointOfInterestById

            return
                {
                    Id = hike.Id
                    Trail = hike.Trail
                    StartDate = hike.StartDate
                    CampPoints = points
                }
        }
        |> App.mapError (DatabaseError "Couldn't map trail points of interest." |> always)

    let getHikeByTrailName (username: string) (trailName: string) =
        app {
            let! { Environment = { ConnectionString = ConnectionString connStr } } = App.ask

            use! conn = HikeRepoDb.openConnection connStr

            use cmd = conn.CreateCommand()
            cmd.CommandText <- "SELECT id, details FROM hike WHERE trail = $trail AND json_extract(details, '$.username') = $username LIMIT 1;"
            cmd.Parameters.AddWithValue("$trail", trailName) |> ignore
            cmd.Parameters.AddWithValue("$username", username) |> ignore

            return!
                HikeRepoDb.withReader cmd (fun rdr ->
                    let hikeId = HikeRepoDb.toInt64 (rdr.GetValue 0)
                    let details = rdr.GetString 1
                    let parsed = JsonSerializer.Deserialize<Hike>(details)
                    { parsed with Id = hikeId })
        }

    let getHikeById (username: string) (id: int64) =
        app {
            let! { Environment = { ConnectionString = ConnectionString connStr } } = App.ask

            use! conn = HikeRepoDb.openConnection connStr

            use cmd = conn.CreateCommand()
            cmd.CommandText <- "SELECT id, details FROM hike WHERE id = $id AND json_extract(details, '$.username') = $username LIMIT 1;"
            cmd.Parameters.AddWithValue("$id", id) |> ignore
            cmd.Parameters.AddWithValue("$username", username) |> ignore

            let! hike =
                HikeRepoDb.withReader cmd (fun rdr ->
                    let hikeId = HikeRepoDb.toInt64 (rdr.GetValue 0)
                    let details = rdr.GetString 1
                    let options = JsonSerializerOptions(PropertyNameCaseInsensitive = true)
                    let parsed = JsonSerializer.Deserialize<Hike>(details, options)
                    { parsed with Id = hikeId })

            return! withPoints hike
        }

    let getHikes (username: string) =
        app {
            let! { Environment = { ConnectionString = ConnectionString connStr } } = App.ask

            use! conn = HikeRepoDb.openConnection connStr

            use cmd = conn.CreateCommand()
            cmd.CommandText <- "SELECT id, details FROM hike WHERE json_extract(details, '$.username') = $username ORDER BY id;"
            cmd.Parameters.AddWithValue("$username", username) |> ignore

            let rec readAllHikes hikes (rdr: DbDataReader) =
                let hikeId = HikeRepoDb.toInt64 (rdr.GetValue 0)
                let details = rdr.GetString 1
                let options = JsonSerializerOptions(PropertyNameCaseInsensitive = true)
                let parsed = JsonSerializer.Deserialize<Hike>(details, options)
                let hike = { parsed with Id = hikeId }

                if rdr.Read() then
                    readAllHikes (hike :: hikes) rdr
                else
                    hike :: hikes

            let! hikes = HikeRepoDb.withReader cmd (fun rdr -> readAllHikes [] rdr)

            return! hikes |> List.map withPoints
        }
