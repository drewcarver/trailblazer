module HikePlanner.Views.Hikes.ListHikes

open Giraffe.ViewEngine
open HikePlanner.Views.MasterLayout
open HikePlanner.Repositories.HikeRepoTypes
open HikePlanner.Views.Components.Table
open HikePlanner.Core
open HikePlanner.Core
open HikePlanner.Core.Utils

let myHikesTable = 
  trailblazerTable 
    "My Hikes" 
    (trailblazerTableHeader ["Hike Name"; "Start Date"; "Camp Locations"; "Action"]) 

let savedHikeRow (hike: SavedHike) =
    trailblazerTableRow [
        trailblazerTableColumn (StringValue hike.Trail)
        trailblazerTableColumn (StringValue (hike.StartDate.ToString "yyyy-MM-dd"))
        trailblazerTableColumn (StringValue (
            sprintf "%s to %s" 
                (hike.CampPoints |> List.head |> fun point -> point.Name) 
                (hike.CampPoints |> List.last |> fun point -> point.Name)
            )
        )
        td [ _class "px-4 py-3 text-center whitespace-nowrap" ] [
            a [ _href (sprintf "/hikes/%d" hike.Id); _class "inline-flex items-center justify-center px-3 py-1 text-xs font-mono font-bold uppercase border border-black bg-neutral-100 hover:bg-black hover:text-white shadow-[2px_2px_0px_0px_rgba(0,0,0,1)] active:translate-x-[1px] active:translate-y-[1px] active:shadow-[1px_1px_0px_0px_rgba(0,0,0,1)] transition-all cursor-pointer" ] [ str "Edit Hike" ]
        ]
    ]

let emptyHikeRow =
    trailblazerTableRow [
        trailblazerTableColumn (StringValue "")
        trailblazerTableColumn (StringValue "")
        trailblazerTableColumn (StringValue "")
        trailblazerTableColumn (StringValue "")
    ]

let hikingTable hikes = 
    let populatedHikes = hikes |> List.map savedHikeRow
    populatedHikes @ List.init (10 - populatedHikes.Length) (always emptyHikeRow)
        |> myHikesTable 

let errorOcurredTable error = 
    myHikesTable [
        trailblazerTableRow [ span [] [ str error ] ] 
    ]

let noHikesAvailableTable =
    myHikesTable [
        trailblazerTableRow [ span [] [ str "No hikes available." ]]
    ]

let listHikes userProfile hikesResult = 
    let renderTable hikes = 
        hikingTable hikes

    match hikesResult with 
    | Ok hikes  -> renderTable hikes
    | Error (NotFound _) -> renderTable []
    | Error (DatabaseError e | FormValidationError e) -> errorOcurredTable e
    |> XmlNodeBody |> withMasterLayout userProfile
