#r "nuget: Microsoft.Data.Sqlite"

open Microsoft.Data.Sqlite

let dbPath = "hikes.db"
let tableName = "hike"

let connectionString = sprintf "Data Source=%s" dbPath

try
    use connection = new SqliteConnection(connectionString)
    connection.Open()

    // Craft the SQL command safely targeting the table
    let query = sprintf "DROP TABLE IF EXISTS %s;" tableName
    use command = new SqliteCommand(query, connection)
    
    let rowsAffected = command.ExecuteNonQuery()
    printfn "Successfully dropped table '%s' (if it existed)." tableName

with
| :? SqliteException as ex -> 
    printfn "SQLite Error: %s" ex.Message
| ex -> 
    printfn "An unexpected error occurred: %s" ex.Message
