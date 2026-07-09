module HikePlanner.Views.Plan.ListPlans

open Giraffe.ViewEngine
open HikePlanner.Views.MasterLayout
open HikePlanner.Repositories.HikeRepo
open HikePlanner.Views.Components.Table
open HikePlanner.Core

let hikingTable (hikes: Hike list) = 
  trailblazerTable 
    "My Hikes" 
    (trailblazerTableHeader ["Hike Name"; "Start Date"; "End Date"; "Action"]) 
    (hikes |> List.map (fun h -> 
    trailblazerTableRow [
        trailblazerTableColumn (StringValue h.Trail)
        trailblazerTableColumn (StringValue (h.StartDate.ToString "yyyy-MM-dd"))
        trailblazerTableColumn (StringValue (h.EndDate.ToString "yyyy-MM-dd"))
        trailblazerTableButton (sprintf "View details for %s" h.Trail) "View Hike"
    ]
    ))

let listPlans hikes = 
    hikingTable hikes
    |> withMasterLayout
