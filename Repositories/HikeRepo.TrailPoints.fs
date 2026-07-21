namespace HikePlanner.Repositories

module HikeRepoTrailPoints =
    open Turso.Data.Sqlite
    open HikePlanner.Core
    open HikePlanner.Infrastructure
    open HikePlanner.Repositories.HikeRepoTypes

    let getTrailPointsOfInterest (trailName: string) =
        app {
            let! { Environment = { ConnectionString = ConnectionString connStr } } = App.ask

            use conn = new SqliteConnection(connStr)
            conn.Open() |> ignore

            use cmd = conn.CreateCommand()
            cmd.CommandText <- "SELECT Id, TrailName, TrailMile, Name FROM TrailPointsOfInterest WHERE TrailName = $trailName ORDER BY TrailMile;"
            cmd.Parameters.AddWithValue("$trailName", trailName) |> ignore

            return!
                App.catch
                    (fun ex -> DatabaseError (sprintf "Error retrieving points of interest: %s" ex.Message))
                    (fun _ ->
                        use rdr = cmd.ExecuteReader()

                        [ while rdr.Read() do
                              let id = HikeRepoDb.toInt64 (rdr.GetValue 0)
                              let pointTrailName = rdr.GetString 1
                              let trailMile = rdr.GetDouble 2
                              let name = rdr.GetString 3

                              yield
                                  {
                                      Id = id
                                      TrailName = pointTrailName
                                      TrailMile = trailMile
                                      Name = name
                                  } ])
        }

    let getTrailPointOfInterestById (id: int64) =
        app {
            let! { Environment = { ConnectionString = ConnectionString connStr } } = App.ask

            use conn = new SqliteConnection(connStr)
            let! _ = conn.OpenAsync()
            HikeRepoDb.ensureHikesTable conn

            use cmd = conn.CreateCommand()
            cmd.CommandText <- "SELECT Id, TrailName, TrailMile, Name FROM TrailPointsOfInterest WHERE Id = $id ORDER BY TrailMile LIMIT 1;"
            cmd.Parameters.AddWithValue("$id", id) |> ignore

            return!
                HikeRepoDb.withReader cmd (fun rdr ->
                    let pointId = HikeRepoDb.toInt64 (rdr.GetValue 0)
                    let pointTrailName = rdr.GetString 1
                    let trailMile = rdr.GetDouble 2
                    let name = rdr.GetString 3

                    {
                        Id = pointId
                        TrailName = pointTrailName
                        TrailMile = trailMile
                        Name = name
                    })
        }
