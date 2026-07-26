module HikePlanner.Views.Hikes.ListHikesView

open HikePlanner.Views.MasterLayout
open HikePlanner.Views.Components.Table

let listHikesView userProfile =
    let table = trailblazerSkeletonTable "My Hikes" 4 10 
    withMasterLayout userProfile (XmlNodeBody table)
