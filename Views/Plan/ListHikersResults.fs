module HikePlanner.Views.Plan.ListHikersResults

open Giraffe.ViewEngine
open HikePlanner.Views.MasterLayout
open HikePlanner.Repositories.HikeRepo
open HikePlanner.Core

let listHikersResultsView userProfile (hikersResult: Result<User, TrailblazerError>) =
    match hikersResult with
    | Ok hiker -> hiker.Friends 
                    |> List.map (fun friend -> option [ _value friend.Email ] [ str friend.Email ])
    | Error (NotFound error) -> [option [ _value "" ] [ str error ]]
    |> XmlNodeList |>withMasterLayout userProfile