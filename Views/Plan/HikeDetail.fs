module HikePlanner.Views.Plan.HikeDetail

open Giraffe.ViewEngine
open HikePlanner.Views.MasterLayout
open HikePlanner.Repositories.HikeRepo
open HikePlanner.Core

let hikeDetailView userProfile (hikeResult: Result<SavedHike, TrailblazerError>) = 
    match hikeResult with
    | Ok hike ->
        [
            h1 [ _class "text-2xl font-bold mb-4" ] [ str "Hike Details" ]
            div [ _class "max-w-3xl mx-auto mt-8 p-6 bg-white rounded-3xl shadow-md border border-[#D4C3A8]" ] [
                div [ _class "mb-4" ] [
                    h2 [ _class "text-xl font-bold mb-2" ] [ str "Hike Name" ]
                    p [ _class "text-lg" ] [ str hike.Trail ]
                ]
                div [ _class "mb-4" ] [
                    h2 [ _class "text-xl font-bold mb-2" ] [ str "Start Date" ]
                    p [ _class "text-lg" ] [ str (hike.StartDate.ToString "yyyy-MM-dd") ]
                ]
            ]
            a [ _href "/plan"; _class "inline-flex items-center justify-center px-4 py-2 text-sm font-mono font-bold uppercase border border-black bg-neutral-100 hover:bg-black hover:text-white shadow-[2px_2px_0px_0px_rgba(0,0,0,1)] active:translate-x-[1px] active:translate-y-[1px] active:shadow-[1px_1px_0px_0px_rgba(0,0,0,1)] transition-all cursor-pointer mt-4" ] [ str "Back to Plans" ]
        ]
    | Error error ->
        [
            h1 [ _class "text-2xl font-bold mb-4" ] [ str "Error" ]
            p [ _class "text-lg" ] [ str "An error occurred while retrieving the hike details." ]
            a [ _href "/plan"; _class "inline-flex items-center justify-center px-4 py-2 text-sm font-mono font-bold uppercase border border-black bg-neutral-100 hover:bg-black hover:text-white shadow-[2px_2px_0px_0px_rgba(0,0,0,1)] active:translate-x-[1px] active:translate-y-[1px] active:shadow-[1px_1px_0px_0px_rgba(0,0,0,1)] transition-all cursor-pointer mt-4" ] [ str "Back to Plans" ]
        ]
    |> XmlNodeList |> withMasterLayout userProfile 