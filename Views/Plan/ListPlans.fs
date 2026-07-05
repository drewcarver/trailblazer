module HikingPlanner.Views.Plan.ListPlans  

open Giraffe.ViewEngine
open HikePlanner.Views.MasterLayout
open HikePlanner.Repositories.HikeRepo
open SharedModules

let hikingTable (hikes: Hike list) = 
  odysseeTable 
    "My Hikes" 
    (odysseeTableHeader ["Hike Name"; "Start Date"; "End Date"; "Action"]) 
    (hikes |> List.map (fun h -> 
    odysseeTableRow [
        odysseeTableColumn (StringValue h.Trail)
        odysseeTableColumn (StringValue (h.StartDate.ToString "yyyy-MM-dd"))
        odysseeTableColumn (StringValue (h.EndDate.ToString "yyyy-MM-dd"))
        odysseeTableButton (sprintf "View details for %s" h.Trail) "View Hike"
    ]
    ))

let listPlans (hikes: Result<Hike list, HikeRepoError>)  = 
    match hikes with
      | Ok h -> hikingTable h
      | Error e -> div [] [ 
          match e with
            | DatabaseError e -> str e 
            | NotFound e -> str e 
      ] 
    |> withMasterLayout
