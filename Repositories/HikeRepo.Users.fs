namespace HikePlanner.Repositories

module HikeRepoUsers =
    open HikePlanner.Core
    open HikePlanner.Infrastructure
    open System.Text.Json

    let saveUser (user: User) =
        app {
            let! { Environment = { ConnectionString = ConnectionString connStr } } = App.ask

            use! conn = HikeRepoDb.openConnection connStr

            use cmd = conn.CreateCommand()
            cmd.CommandText <- "INSERT OR REPLACE INTO user (email, details) VALUES ($email, $details);"
            cmd.Parameters.AddWithValue("$email", user.Email) |> ignore
            cmd.Parameters.AddWithValue("$details", JsonSerializer.Serialize user) |> ignore

            try
                let rowsAffected = cmd.ExecuteNonQuery()
                if rowsAffected > 0 then
                    return! App.succeed ()
                else
                    return! App.fail (DatabaseError "No rows were affected when saving the user.")
            with ex ->
                return! App.fail (DatabaseError (sprintf "Error saving user: %s" ex.Message))
        }

    let getUser userName =
        app {
            let! { Environment = { ConnectionString = ConnectionString connStr } } = App.ask

            use! conn = HikeRepoDb.openConnection connStr

            use cmd = conn.CreateCommand()
            cmd.CommandText <-
                """
                SELECT email, details
                FROM user
                WHERE email = $email
                LIMIT 1;
                """
            cmd.Parameters.AddWithValue("$email", userName) |> ignore

            let! hiker =
                HikeRepoDb.withReader cmd (fun rdr ->
                    let email = rdr.GetString 0
                    let hikerJson = rdr.GetString 1
                    let options = JsonSerializerOptions(PropertyNameCaseInsensitive = true)
                    let hiker = JsonSerializer.Deserialize<User>(hikerJson, options)
                    { hiker with Email = email })

            return hiker
        }
