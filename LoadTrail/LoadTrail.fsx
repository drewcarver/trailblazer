#r "nuget: Microsoft.Data.Sqlite"
#r "nuget: FSharp.Data"

open System
open System.IO
open Microsoft.Data.Sqlite
open FSharp.Data

let dbName = "hikes2.db"

type Record = {
    TrailName : string
    TrailMile : double
    Name : string
}

let processAndInsert (csvPath: string) (dbName: string) =
    let mutable conn: SqliteConnection = null
    try
        if not (File.Exists(csvPath)) then
            raise (FileNotFoundException($"Error: {csvPath} not found."))

        let csv = CsvFile.Load(csvPath)
        
        let firstRow = csv.Rows |> Seq.tryHead
        match firstRow with
        | None -> 
            printfn "Error: No valid records processed for insertion."
        | Some row when row.Columns.Length < 2 -> 
            printfn "Error: CSV must have at least two columns."
        | Some _ ->
            printfn "CSV Head:"
            csv.Rows |> Seq.truncate 5 |> Seq.iter (fun r -> printfn "%A" r.Columns)

            let dataToInsert = 
                csv.Rows
                |> Seq.choose (fun row ->
                    // Try parsing the second column as a float (TrailMile)
                    match Double.TryParse(row.[1]) with
                    | true, mile -> 
                        Some { TrailName = "AppalachianTrail"; TrailMile = mile; Name = row.[0] }
                    | _ -> 
                        printfn "Warning: Skipping row due to invalid float format: %s" row.[1]
                        None)
                |> Seq.toList

            if List.isEmpty dataToInsert then
                printfn "Error: No valid records processed for insertion."
            else
                conn <- new SqliteConnection($"Data Source={dbName}")
                conn.Open()

                use cmd = conn.CreateCommand()
                
                cmd.CommandText <- "
                    CREATE TABLE IF NOT EXISTS TrailPointsOfInterest (
                        ID INTEGER PRIMARY KEY,
                        Name TEXT NOT NULL,
                        TrailName TEXT NOT NULL,
                        TrailMile REAL NOT NULL
                    );
                    CREATE INDEX IF NOT EXISTS idx_trail_points ON TrailPointsOfInterest (TrailName, TrailMile);
                "
                cmd.ExecuteNonQuery() |> ignore

                use transaction = conn.BeginTransaction()
                try
                    use insertCmd = conn.CreateCommand()
                    insertCmd.Transaction <- transaction
                    insertCmd.CommandText <- "
                        INSERT INTO TrailPointsOfInterest (TrailName, TrailMile, Name) 
                        VALUES ($trailName, $trailMile, $name)
                    "
                    
                    let pTrailName = insertCmd.Parameters.Add("$trailName", SqliteType.Text)
                    let pTrailMile = insertCmd.Parameters.Add("$trailMile", SqliteType.Real)
                    let pName = insertCmd.Parameters.Add("$name", SqliteType.Text)

                    for record in dataToInsert do
                        pTrailName.Value <- record.TrailName
                        pTrailMile.Value <- record.TrailMile
                        pName.Value <- record.Name
                        insertCmd.ExecuteNonQuery() |> ignore

                    transaction.Commit()
                    printfn "Success: %d records inserted into %s." dataToInsert.Length dbName

                with :? SqliteException as e ->
                    transaction.Rollback()
                    printfn "SQLite Error during insertion: %s" e.Message

    with 
    | :? FileNotFoundException as e -> 
        printfn "%s" e.Message
    | e -> 
        printfn "Critical error during operation: %s" e.Message

processAndInsert "AppalachianTrailLocations.csv" dbName
