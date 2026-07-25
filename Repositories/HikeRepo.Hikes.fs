namespace HikePlanner.Repositories

module HikeRepoHikes =
    open System
    open HikePlanner.Core
    open HikePlanner.Core.Utils
    open HikePlanner.Infrastructure
    open HikePlanner.Repositories.HikeRepoTypes
    open System.Text.Json
    open System.Data.Common

    let private toHikeJson (trail: string) (startDate: DateTime) (campPoints: int list) =
        {| Trail = trail
           StartDate = startDate
           campPoints = campPoints |}
        |> JsonSerializer.Serialize

    let saveHike (trail: string) (startDate: DateTime) (campPoints: int list) =
        app {
            let! ConnectionString connStr = App.asks (fun env -> env.Environment.ConnectionString)

            use! conn = HikeRepoDb.openConnection connStr

            let hikeDetails = toHikeJson trail startDate campPoints

            use cmd = conn.CreateCommand()
            cmd.CommandText <- "INSERT INTO hike (details) VALUES ($details); SELECT last_insert_rowid();"
            cmd.Parameters.AddWithValue("$details", hikeDetails) |> ignore

            try
                let id = cmd.ExecuteScalar()
                return! App.succeed id
            with ex ->
                return! App.fail (DatabaseError (sprintf "Error saving hike: %s" ex.Message))
        }

    let updateHike (id: int64) (trail: string) (startDate: DateTime) (campPoints: int list) =
        app {
            let! ConnectionString connStr = App.asks (fun env -> env.Environment.ConnectionString)

            use! conn = HikeRepoDb.openConnection connStr

            let hikeDetails = toHikeJson trail startDate campPoints

            use cmd = conn.CreateCommand()
            cmd.CommandText <- "UPDATE hike SET details = $details WHERE id = $id;"
            cmd.Parameters.AddWithValue("$details", hikeDetails) |> ignore
            cmd.Parameters.AddWithValue("$id", id) |> ignore

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

    let getHikeByTrailName (trailName: string) =
        app {
            let! { Environment = { ConnectionString = ConnectionString connStr } } = App.ask

            use! conn = HikeRepoDb.openConnection connStr

            use cmd = conn.CreateCommand()
            cmd.CommandText <- "SELECT id, details FROM hike WHERE trail = $trail LIMIT 1;"
            cmd.Parameters.AddWithValue("$trail", trailName) |> ignore

            return!
                HikeRepoDb.withReader cmd (fun rdr ->
                    let hikeId = HikeRepoDb.toInt64 (rdr.GetValue 0)
                    let details = rdr.GetString 1
                    let parsed = JsonSerializer.Deserialize<Hike>(details)
                    { parsed with Id = hikeId })
        }

    let getHikeById (id: int64) =
        app {
            let! { Environment = { ConnectionString = ConnectionString connStr } } = App.ask

            use! conn = HikeRepoDb.openConnection connStr

            use cmd = conn.CreateCommand()
            cmd.CommandText <- "SELECT id, details FROM hike WHERE id = $id LIMIT 1;"
            cmd.Parameters.AddWithValue("$id", id) |> ignore

            let! hike =
                HikeRepoDb.withReader cmd (fun rdr ->
                    let hikeId = HikeRepoDb.toInt64 (rdr.GetValue 0)
                    let details = rdr.GetString 1
                    let options = JsonSerializerOptions(PropertyNameCaseInsensitive = true)
                    let parsed = JsonSerializer.Deserialize<Hike>(details, options)
                    { parsed with Id = hikeId })

            return! withPoints hike
        }

    let getHikes =
        app {
            let! { Environment = { ConnectionString = ConnectionString connStr } } = App.ask

            use! conn = HikeRepoDb.openConnection connStr

            use cmd = conn.CreateCommand()
            cmd.CommandText <- "SELECT id, details FROM hike ORDER BY id;"

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
