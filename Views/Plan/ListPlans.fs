module HikePlanner.Views.ListPlans

open Giraffe.ViewEngine
open HikePlanner.Views.MasterLayout
open HikePlanner.Repositories.HikeRepo

let hikeRow (hike: Hike) = 
  tr [] [
    td [] [ str hike.Trail ]
  ]

let listPlans (hikes: Result<Hike list, HikeRepoError>)  = 
    body [ _class "bg-[#EDE4D5] text-gray-800" ] [
        nav [ _class "bg-[#2E5A3D] text-white sticky top-0 z-50 shadow-md" ] [
            div [ _class "max-w-7xl mx-auto px-6 py-4 flex items-center justify-between" ] [
                div [ _class "flex items-center gap-3" ] [
                    i [ _class "fa-solid fa-mountain text-3xl text-[#EDE4D5]" ] []
                    div [ _class "logo-font text-3xl font-bold tracking-tight" ] [ str "TrailForge" ]
                ]
                div [ _class "hidden md:flex items-center gap-8 text-sm font-medium" ] [
                    a [ _href "#"; _class "hover:text-[#D4C3A8] transition-colors" ] [ str "Discover Trails" ]
                    a [ _href "#"; _class "hover:text-[#D4C3A8] transition-colors" ] [ str "Plan Hike" ]
                    a [ _href "#"; _class "hover:text-[#D4C3A8] transition-colors" ] [ str "My Journal" ]
                    a [ _href "#"; _class "hover:text-[#D4C3A8] transition-colors" ] [ str "Past Hikes" ]
                    a [ _href "#"; _class "hover:text-[#D4C3A8] transition-colors" ] [ str "Community" ]
                ]
            ]
        ]
        match hikes with
          | Ok h -> table [] (h |> List.map hikeRow)
          | Error e -> div [] [ 
              match e with
                | DatabaseError e -> str e 
                | NotFound e -> str e 
          ] 
    ] |> withMasterLayout
