module HikePlanner.Views.Plan.ListPlans

open Giraffe.ViewEngine
open HikePlanner.Views.MasterLayout
open HikePlanner.Repositories.HikeRepo
open HikePlanner.Views.Components.Table
open HikePlanner.Core

let myHikesTable = 
  trailblazerTable 
    "My Hikes" 
    (trailblazerTableHeader ["Hike Name"; "Start Date"; "End Date"; "Action"]) 

let myHikeRow hike =
    trailblazerTableRow [
        trailblazerTableColumn (StringValue hike.Trail)
        trailblazerTableColumn (StringValue (hike.StartDate.ToString "yyyy-MM-dd"))
        trailblazerTableColumn (StringValue (hike.EndDate.ToString "yyyy-MM-dd"))
        trailblazerTableButton (sprintf "View details for %s" hike.Trail) "View Hike"
    ]

let hikingTable (hikes: Hike list) = myHikesTable (hikes |> List.map myHikeRow)

let noHikesAvailableTable = 
    myHikesTable [
        trailblazerTableRow [ span [] [ str "Couldn't connect to the database." ] ] 
    ]

let listPlans (hikesResult: Result<Hike list, TrailblazerError>) = 
    match hikesResult with 
    | Ok hikes -> hikingTable hikes
    | Error e  -> noHikesAvailableTable
    |> withMasterLayout
