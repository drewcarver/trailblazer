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
    (trailblazerTableHeader ["Hike Name"; "Date Range"; "Trail"; "Start Location"; "End Location"; "Actions"]) 

let private locationCell (point: TrailPointOfInterest) =
    XmlNodeValue (
        span [ _class "table__location" ] [
            i [ _class "fa-solid fa-location-dot"; attr "aria-hidden" "true" ] []
            str point.Name
        ]
    )

let private hikeNameCell (hike: SavedHike) =
    XmlNodeValue (
        div [ _class "table__name" ] [
            span [] [ str hike.Trail ]
        ]
    )

let private trailName (hike: SavedHike) =
    hike.CampPoints
    |> List.tryHead
    |> Option.map (fun point -> point.TrailName)
    |> Option.defaultValue hike.Trail

let savedHikeRow (hike: SavedHike) =
    let startPoint, endPoint =
        match hike.CampPoints with
        | first :: rest -> first, (rest |> List.rev |> List.tryHead |> Option.defaultValue first)
        | [] ->
            let fallback: TrailPointOfInterest =
                { Id = 0L; Name = "No camp location"; TrailName = hike.Trail; TrailMile = 0.0 }
            fallback, fallback

    trailblazerTableRow [
        trailblazerTableColumn (hikeNameCell hike)
        trailblazerTableColumn (XmlNodeValue (span [ _class "table__date" ] [
            i [ _class "fa-regular fa-calendar"; attr "aria-hidden" "true" ] []
            str (" " + hike.StartDate.ToString "MMM d, yyyy")
        ]))
        trailblazerTableColumn (StringValue (trailName hike))
        trailblazerTableColumn (locationCell startPoint)
        trailblazerTableColumn (locationCell endPoint)
        td [ _class "table__action-cell" ] [
            a [ _href (sprintf "/hikes/%d" hike.Id); _class "table__action"; attr "aria-label" (sprintf "Edit %s" hike.Trail) ] [
                i [ _class "fa-solid fa-pen"; attr "aria-hidden" "true" ] []
            ]
        ]
    ]

let emptyHikeRow =
    trailblazerTableRow [
        trailblazerTableColumn (StringValue "")
        trailblazerTableColumn (StringValue "")
        trailblazerTableColumn (StringValue "")
        trailblazerTableColumn (StringValue "")
        trailblazerTableColumn (StringValue "")
        trailblazerTableColumn (StringValue "")
    ]

let hikingTable hikes = 
    let populatedHikes = hikes |> List.map savedHikeRow
    populatedHikes @ List.init (max 0 (10 - populatedHikes.Length)) (always emptyHikeRow)
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
