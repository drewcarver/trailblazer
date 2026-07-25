#r "nuget: Nelknet.LibSQL.Data, 0.2.11"

open System
open System.IO
open Nelknet.LibSQL.Data

let loadEnvFile (path: string) =
    if File.Exists(path) then
        File.ReadAllLines(path)
        |> Array.iter (fun line ->
            let trimmed = line.Trim()
            if not (String.IsNullOrWhiteSpace(trimmed)) && not (trimmed.StartsWith("#")) then
                let idx = trimmed.IndexOf('=')
                if idx > 0 then
                    let key = trimmed.Substring(0, idx).Trim()
                    let value = trimmed.Substring(idx + 1).Trim().Trim('"')
                    Environment.SetEnvironmentVariable(key, value))

let projectRoot = Path.GetFullPath(Path.Combine(__SOURCE_DIRECTORY__, ".."))
let envPath = Path.Combine(projectRoot, ".env")
loadEnvFile envPath

let dbUrl = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")
let authToken = Environment.GetEnvironmentVariable("TURSO_AUTH_TOKEN")

if String.IsNullOrWhiteSpace dbUrl then
    failwith "DB_CONNECTION_STRING is missing in .env"

if String.IsNullOrWhiteSpace authToken then
    failwith "TURSO_AUTH_TOKEN is missing in .env"

let connectionString = $"Data Source={dbUrl};Auth Token={authToken}"

printfn "Connecting with Nelknet.LibSQL.Data to %s" dbUrl

use connection = new LibSQLConnection(connectionString)
connection.Open()

use createTable = connection.CreateCommand()
createTable.CommandText <- "CREATE TABLE IF NOT EXISTS sample (id INTEGER PRIMARY KEY AUTOINCREMENT, name TEXT NOT NULL);"
createTable.ExecuteNonQuery() |> ignore

use insertRow = connection.CreateCommand()
insertRow.CommandText <- "INSERT INTO sample (name) VALUES (@name);"
let nameParam = insertRow.CreateParameter()
nameParam.ParameterName <- "@name"
nameParam.Value <- "nelknet-smoke"
insertRow.Parameters.Add(nameParam) |> ignore
let inserted = insertRow.ExecuteNonQuery()

use countCmd = connection.CreateCommand()
countCmd.CommandText <- "SELECT COUNT(*) FROM sample WHERE name = @name;"
let countParam = countCmd.CreateParameter()
countParam.ParameterName <- "@name"
countParam.Value <- "nelknet-smoke"
countCmd.Parameters.Add(countParam) |> ignore
let count = Convert.ToInt32(countCmd.ExecuteScalar())

printfn "Success: created sample and inserted %d row(s); matching rows = %d" inserted count
