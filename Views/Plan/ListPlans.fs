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
        td [ _class "px-4 py-3 text-center whitespace-nowrap" ] [
            a [ _href (sprintf "/plan/%d" hike.Id); _class "inline-flex items-center justify-center px-3 py-1 text-xs font-mono font-bold uppercase border border-black bg-neutral-100 hover:bg-black hover:text-white shadow-[2px_2px_0px_0px_rgba(0,0,0,1)] active:translate-x-[1px] active:translate-y-[1px] active:shadow-[1px_1px_0px_0px_rgba(0,0,0,1)] transition-all cursor-pointer" ] [ str "View Hike" ]
        ]
    ]

let hikingTable (hikes: Hike list) = myHikesTable (hikes |> List.map myHikeRow)

let noHikesAvailableTable = 
    myHikesTable [
        trailblazerTableRow [ span [] [ str "Couldn't connect to the database." ] ] 
    ]

let listPlans (hikesResult: Result<Hike list, TrailblazerError>) = 
    match hikesResult with 
    | Ok hikes -> 
        div [] [
            a [ _href "/plan/create"; _class "inline-flex items-center justify-center px-4 py-2 text-sm font-mono font-bold uppercase border border-black bg-neutral-100 hover:bg-black hover:text-white shadow-[2px_2px_0px_0px_rgba(0,0,0,1)] active:translate-x-[1px] active:translate-y-[1px] active:shadow-[1px_1px_0px_0px_rgba(0,0,0,1)] transition-all cursor-pointer mb-4" ] [ str "Create New Plan" ]
            hikingTable hikes
        ]
    | Error e  -> noHikesAvailableTable
    |> withMasterLayout
