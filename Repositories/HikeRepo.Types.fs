namespace HikePlanner.Repositories

module HikeRepoTypes =
    open System

    type Hike = {
        Id: int64
        Trail: string
        StartDate: DateTime
        CampPoints: int64 list
    }

    type TrailPointOfInterest = {
        Id: int64
        Name: string
        TrailName: string
        TrailMile: float
    }

    type SavedHike = {
        Id: int64
        Trail: string
        StartDate: DateTime
        CampPoints: TrailPointOfInterest list
    }
